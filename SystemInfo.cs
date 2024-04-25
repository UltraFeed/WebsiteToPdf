#pragma warning disable CA1305
#pragma warning disable IDE0058

using System.Management;
using System.Net;
using System.Net.Sockets;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace WebsiteToPdf;
internal sealed class SystemInfo
{

	internal static async Task CreateSysInfoPdf (string url, string pdfPathText, string pdfPathScreen)
	{
		byte [] imageBytes = await Utilities.TakeScreenshot(url, pdfPathScreen).ConfigureAwait(false);

		using FileStream fs = new(pdfPathText, FileMode.Create);
		using PdfWriter writer = new(fs);
		using PdfDocument pdf = new(writer);
		using Document doc = new(pdf);

		string osInfo = GetOsInfo();
		string currentTime = $"Time UTC+0 = {GetNtpTime("pool.ntp.org")}";
		string ipAddress = await GetExternalIpAddress().ConfigureAwait(false);

		Paragraph osPara = new(osInfo);
		Paragraph timePara = new(currentTime);
		Paragraph ipPara = new($"IP Address: {ipAddress}");
		Paragraph infoPara = new($"Program took screenshot of {url}");

		doc.Add(osPara);
		doc.Add(timePara);
		doc.Add(ipPara);
		doc.Add(infoPara);

		// Новый метод - растягиваем страницу до размеров скриншота
		Image img = new(ImageDataFactory.Create(imageBytes));
		PageSize pageSize = new(img.GetImageWidth(), img.GetImageHeight());
		pdf.SetDefaultPageSize(pageSize);

		// Старый метод - весь скриншот ужимается до размеров страницы
		//Image img = new(ImageDataFactory.Create(imageBytes));
		//img.ScaleToFit(pdf.GetDefaultPageSize().GetWidth(), pdf.GetDefaultPageSize().GetHeight());

		doc.Add(img);
		doc.Close();
	}

	internal static string GetOsInfo ()
	{
		using ManagementObjectSearcher searcher = new("SELECT * FROM Win32_OperatingSystem");
		ManagementObjectCollection osCollection = searcher.Get();
		string osInfo = "";

		foreach (ManagementObject os in osCollection.Cast<ManagementObject>())
		{
			PropertyDataCollection properties = os.Properties;
			foreach (PropertyData property in properties)
			{
				osInfo += $"{property.Name}: {property.Value}\n";
			}
		}

		return osInfo;
	}

	internal static async Task<string> GetExternalIpAddress ()
	{
		Uri IpApiUri = new("https://api.ipify.org?format=text");
		using HttpClient httpClient = new();
		return await httpClient.GetStringAsync(IpApiUri).ConfigureAwait(false);
	}

	internal static string GetNtpTime (string ntpServer)
	{
		byte [] ntpData = new byte [48];
		ntpData [0] = 0x1B; //LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

		IPAddress [] addresses = Dns.GetHostEntry(ntpServer).AddressList;
		IPEndPoint ipEndPoint = new(addresses [0], 123);
		Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

		socket.Connect(ipEndPoint);
		socket.Send(ntpData);
		socket.Receive(ntpData);
		socket.Close();

		ulong intPart = ((ulong) ntpData [40] << 24) | ((ulong) ntpData [41] << 16) | ((ulong) ntpData [42] << 8) | ntpData [43];
		ulong fractPart = ((ulong) ntpData [44] << 24) | ((ulong) ntpData [45] << 16) | ((ulong) ntpData [46] << 8) | ntpData [47];

		ulong milliseconds = (intPart * 1000) + (fractPart * 1000 / 0x100000000L);
		DateTime networkDateTime = new DateTime(1900, 1, 1).AddMilliseconds((long) milliseconds);

		return networkDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
	}
}
