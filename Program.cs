#pragma warning disable CA1303
#pragma warning disable CS8600

namespace WebsiteToPdf;

internal sealed class Program
{
	internal static async Task Main ()
	{
		bool debug = false;
		Uri website;
		if (debug)
		{
			website = new("https://япомогу.рф");
		}
		else
		{
			Console.Write("Введите URL:");
			string uriString = Console.ReadLine();
			if (Uri.TryCreate(uriString, UriKind.Absolute, out website))
			{
				Console.WriteLine($"URL успешно создан: {website}");
			}
			else
			{
				Console.WriteLine("Ошибка: Введенная строка не является допустимым URL");
				_ = Console.ReadLine();
				Environment.Exit(0);
			}
		}

		string output = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "output.pdf");
		string pdfTemp1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "pdfTemp1.pdf");
		string pdfTemp2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "pdfTemp2.pdf");

		Utilities.RemoveFiles([pdfTemp1, pdfTemp2, output]);

		await SystemInfo.CreatePdfFiles(website, pdfTemp1, pdfTemp2).ConfigureAwait(false);

		Utilities.MergePdfFiles([pdfTemp1, pdfTemp2], output);
		Utilities.RemoveFiles([pdfTemp2, pdfTemp1]);
	}
}