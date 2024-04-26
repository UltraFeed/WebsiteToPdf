#pragma warning disable CA1031
#pragma warning disable CA1305
#pragma warning disable CA1308
#pragma warning disable CS8602
#pragma warning disable CS8604

using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Xml;

internal static class WhoisService
{
	private static readonly Uri whoisServerListUrl = new("https://raw.githubusercontent.com/whois-server-list/whois-server-list/master/whois-server-list.xml");
	private static XmlDocument? serverList;

	internal static async Task<string> SearchInfoAsync (string website)
	{
		if (website.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
		{
			website = website.Remove(0, 8);
		}

		string [] domainLevels = website.Trim().Split('.');
		List<string>? whoisServers = null;

		foreach (string? zone in domainLevels.Skip(1))
		{
			whoisServers = await GetWhoisServersAsync(zone).ConfigureAwait(false);
			if (whoisServers.Count > 0)
			{
				break;
			}
		}

		if (whoisServers == null || whoisServers.Count == 0)
		{
			return $"Unknown domain zone of {website}";
		}
		else
		{
			StringBuilder result = new();
			foreach (string whoisServer in whoisServers)
			{
				_ = result.AppendLine(await LookupAsync(whoisServer, website).ConfigureAwait(false));
			}

			return result.ToString();
		}
	}

	private static async Task<List<string>?> GetWhoisServersAsync (string domainZone)
	{
		if (serverList == null && (serverList = await LoadServerListAsync().ConfigureAwait(false)) == null)
		{
			return null;
		}

		List<string> result = [];
		FindWhoisServers(serverList.DocumentElement, domainZone, result);
		return result;
	}

	private static async Task<XmlDocument?> LoadServerListAsync ()
	{
		try
		{
			using HttpClient client = new();
			string xmlContent = await client.GetStringAsync(whoisServerListUrl).ConfigureAwait(false);
			XmlDocument xmlDoc = new();
			xmlDoc.LoadXml(xmlContent);
			return xmlDoc;
		}

		catch (Exception ex)
		{
			Console.WriteLine($"Error downloading XML: {ex.Message}");
			return null;
		}
	}

	private static void FindWhoisServers (XmlNode? node, string domainZone, List<string> result)
	{
		if (node == null)
		{
			return;
		}

		foreach (XmlNode childNode in node.ChildNodes)
		{
			if (childNode.Name == "domain" && string.Equals(childNode.Attributes? ["name"]?.Value, domainZone, StringComparison.OrdinalIgnoreCase))
			{
				foreach (XmlNode whoisServerNode in childNode.SelectNodes("whoisServer"))
				{
					result.Add(whoisServerNode.Attributes? ["host"]?.Value);
				}
			}

			FindWhoisServers(childNode, domainZone, result);
		}
	}

	private static async Task<string> LookupAsync (string whoisServer, string domainName)
	{
		if (string.IsNullOrEmpty(whoisServer) || string.IsNullOrEmpty(domainName))
		{
			return "Invalid input data";
		}

		try
		{
			using TcpClient tcpClient = new();
			await tcpClient.ConnectAsync(whoisServer.Trim(), 43).ConfigureAwait(false);
			using NetworkStream stream = tcpClient.GetStream();
			using StreamReader sr = new(stream, Encoding.UTF8);
			byte [] domainQueryBytes = Encoding.ASCII.GetBytes(ConvertToPunycode(domainName) + "\n");
			await stream.WriteAsync(domainQueryBytes).ConfigureAwait(false);
			StringBuilder result = new();

			_ = result.AppendLine("---------------------------------------------------------------------");
			_ = result.AppendLine($"According to {whoisServer}:\n");

			string? row;
			while ((row = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
			{
				_ = result.AppendLine(row);
			}

			_ = result.AppendLine("---------------------------------------------------------------------");
			return result.ToString();
		}
		catch (Exception ex)
		{
			return $"Failed to receive data from server {whoisServer}: {ex.Message}";
		}
	}

	internal static string ConvertToPunycode (string domainName)
	{
		return domainName.ToLowerInvariant()
			// Если в названии домена есть нелатинские буквы и это не цифры и не точка и не тире, например, "россия.рф" то сконвертировать имя в XN--H1ALFFA9F.XN--P1AI
			.Any(v => !"abcdefghijklmnopqrstuvdxyz0123456789.-".Contains(v.ToString(), StringComparison.OrdinalIgnoreCase))
				? new IdnMapping().GetAscii(domainName) //вернуть в Punycode
				: domainName;//вернуть исходный вариант
	}
}
