#pragma warning disable CA1303

namespace WebsiteToPdf;

internal sealed class Program
{
	internal static async Task Main ()
	{
		Uri? website;
		while (true)
		{
#if DEBUG
			website = new("https://ru.wikipedia.org/wiki/C_Sharp");
			break;
#endif

#pragma warning disable CS0162
			Console.Write($"Введите URL:");
#pragma warning restore CS0162
			string? uriString = Console.ReadLine();
			if (Uri.TryCreate(uriString, UriKind.Absolute, out website))
			{
				Console.WriteLine($"URL успешно создан: {website}");
				break;
			}
			else
			{
				Console.WriteLine($"Ошибка: Введенная строка не является допустимым URL");
			}
		}

		string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "output.pdf");
		string pdfTemp1Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "pdfTemp1.pdf");
		string pdfTemp2Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "pdfTemp2.pdf");

		Utilities.RemoveFiles([pdfTemp1Path, pdfTemp2Path, outputPath]);

		await Utilities.CreatePdfFiles(website, pdfTemp1Path, pdfTemp2Path).ConfigureAwait(false);

		Utilities.MergePdfFiles([pdfTemp1Path, pdfTemp2Path], outputPath);
		Utilities.RemoveFiles([pdfTemp2Path, pdfTemp1Path]);
	}
}
