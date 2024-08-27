using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Translation.Text;
using Microsoft.Extensions.Configuration;


//Caroline Uthawong-Burr .Net23


namespace NLP_och_frågetjänster_i_Azure_AI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();
        

            Uri endpoint = new Uri(configuration["QnAServiceURL"]);
            AzureKeyCredential credential = new AzureKeyCredential(configuration["QnAServiceKey"]);
            string projectName = (configuration["projectName"]);
            string deploymentName = (configuration["deploymentName"]);
          


            Uri endpointTranslation = new(configuration["TranslationServiceURL"]);
            string key = configuration["TranslationServiceKey"];
            string region = configuration["TranslationServiceRegion"];

            AzureKeyCredential credentialTranslation = new(key);
            TextTranslationClient clientTranslation = new(credentialTranslation, endpointTranslation, region);

            //QuestionAnsweringClient används för att ansluta till och kommunicera med Azure's tjänst.
            QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);
            //QuestionAnsweringProject används för att specificera vilket projekt och vilken utgåva av
            //projektet som du arbetar med.
            QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);

            Console.WriteLine("Ask a question, type exit to quit. ");

            while (true)
            {
                Console.WriteLine("");
                string question = Console.ReadLine();

                if (question.ToLower() == "exit")
                {
                    break;
                }

                string firstAnswer = string.Empty;
                try
                {
                    Response<AnswersResult> response = client.GetAnswers(question, project);

                    foreach (KnowledgeBaseAnswer answer in response.Value.Answers)
                    {
                        Console.WriteLine($"Q:{question}");
                        Console.WriteLine($"A:{answer.Answer}");

                    }
                    firstAnswer = response.Value.Answers[0].Answer;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Requist erro: {ex.Message}");
                }

                Console.WriteLine("//Do you want the answer in Swedish? (y/n)");
                string languageTranslation = Console.ReadLine();
                if (languageTranslation == "y")
                {
                    try
                    {

                        string from = "en";
                        string targetLanguage = "sv";


                        Response<IReadOnlyList<TranslatedTextItem>> response = clientTranslation.Translate(targetLanguage, firstAnswer);
                        IReadOnlyList<TranslatedTextItem> translations = response.Value;
                        TranslatedTextItem translation = translations.FirstOrDefault();

                        Console.WriteLine($"Detected languages of the input text: {translation?.DetectedLanguage?.Language} with score: {translation?.DetectedLanguage?.Confidence}.");
                        Console.WriteLine($"Text was translated to: '{translation?.Translations?.FirstOrDefault().TargetLanguage}' and the result is: '{translation?.Translations?.FirstOrDefault()?.Text}'.");
                    }
                    catch (RequestFailedException exception)
                    {
                        Console.WriteLine($"Error Code: {exception.ErrorCode}");
                        Console.WriteLine($"Message: {exception.Message}");
                    }
                }
            }
        }
    }
}