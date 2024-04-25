#pragma warning disable CA1303
#pragma warning disable CS8600

namespace WebsiteToPdf;

internal sealed class Program
{
	internal static async Task Main ()
	{
		bool debug = false;
		string url;
		if (debug)
		{
			url = "ru.wikipedia.org/wiki/C_Sharp";
		}
		else
		{
			Console.Write("Enter website: ");
			url = Console.ReadLine();
		}

		url = string.Concat("https://", url);

		string pdfTemp1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "pdfTemp1.pdf");
		string pdfTemp2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "pdfTemp2.pdf");
		string output = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "output.pdf");

		Utilities.RemoveFiles([pdfTemp1, pdfTemp2, output]);

		await SystemInfo.CreateSysInfoPdf(url, pdfTemp1, pdfTemp2).ConfigureAwait(false);
		//await Utilities.TakeScreenshotPdf(url, pdfTemp2).ConfigureAwait(false);

		Utilities.MergePdfFiles([pdfTemp1, pdfTemp2], output);
		Utilities.RemoveFiles([pdfTemp2, pdfTemp1]);
	}
}