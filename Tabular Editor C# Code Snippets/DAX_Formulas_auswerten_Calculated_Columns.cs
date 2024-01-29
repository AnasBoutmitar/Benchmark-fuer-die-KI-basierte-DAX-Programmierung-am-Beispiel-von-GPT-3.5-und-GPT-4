#r "Microsoft.AnalysisServices.Core.dll"
using ToM = Microsoft.AnalysisServices.Tabular;

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
Stopwatch stopWatch = new Stopwatch();
stopWatch.Start();

public class PromptData
{
    public string Prompt_ID { get; set; }
    public string TableLocation { get; set; }
    public string FilterContext { get; set; }
}
public class DataFetcher
{
    private List<PromptData> Prompt_typeCC_Answers;

    public DataFetcher()
    {
        // Initialize the list with data
        Prompt_typeCC_Answers = new List<PromptData>
        {
            new PromptData { Prompt_ID = "Prompt_16_CC_Low_DE", TableLocation = "Kunden", FilterContext = "Kunden[KundenID] = \"ANATR\"" },
            new PromptData { Prompt_ID = "Prompt_16_CC_Low_EN", TableLocation = "Customers", FilterContext = "Customers[CustomerID] = \"ANATR\"" },
            new PromptData { Prompt_ID = "Prompt_17_CC_Low_DE", TableLocation = "Mitarbeiter", FilterContext = "Mitarbeiter[MitarbeiterID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_17_CC_Low_EN", TableLocation = "Employees", FilterContext = "Employees[EmployeeID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_18_CC_Low_DE", TableLocation = "Mitarbeiter", FilterContext = "Mitarbeiter[MitarbeiterID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_18_CC_Low_EN", TableLocation = "Employees", FilterContext = "Employees[EmployeeID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_19_CC_Low_DE", TableLocation = "Mitarbeiter", FilterContext = "Mitarbeiter[MitarbeiterID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_19_CC_Low_EN", TableLocation = "Employees", FilterContext = "Employees[EmployeeID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_20_CC_Low_DE", TableLocation = "Mitarbeiter", FilterContext = "Mitarbeiter[MitarbeiterID] = \"2\"" },
            new PromptData { Prompt_ID = "Prompt_20_CC_Low_EN", TableLocation = "Employees", FilterContext = "Employees[EmployeeID] = \"2\"" },
            new PromptData { Prompt_ID = "Prompt_21_CC_Medium_DE", TableLocation = "Produkte", FilterContext = "Produkte[ProduktID] = \"2\"" },
            new PromptData { Prompt_ID = "Prompt_21_CC_Medium_EN", TableLocation = "Products", FilterContext = "Products[ProductID] = \"2\"" },
            new PromptData { Prompt_ID = "Prompt_22_CC_Medium_DE", TableLocation = "Mitarbeiter", FilterContext = "Mitarbeiter[MitarbeiterID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_22_CC_Medium_EN", TableLocation = "Employees", FilterContext = "Employees[EmployeeID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_23_CC_Medium_DE", TableLocation = "Kategorien", FilterContext = "Kategorien[KategorieID] = \"6\"" },
            new PromptData { Prompt_ID = "Prompt_23_CC_Medium_EN", TableLocation = "Categories", FilterContext = "Categories[CategoryID] = \"6\"" },
            new PromptData { Prompt_ID = "Prompt_24_CC_Medium_DE", TableLocation = "Versender", FilterContext = "Versender[VersenderID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_24_CC_Medium_EN", TableLocation = "Shippers", FilterContext = "Shippers[ShipperID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_25_CC_Medium_DE", TableLocation = "Kunden", FilterContext = "Kunden[KundenID] = \"ANATR\"" },
            new PromptData { Prompt_ID = "Prompt_25_CC_Medium_EN", TableLocation = "Customers", FilterContext = "Customers[CustomerID] = \"ANATR\"" },
            new PromptData { Prompt_ID = "Prompt_26_CC_Hard_DE", TableLocation = "Versender", FilterContext = "Versender[VersenderID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_26_CC_Hard_EN", TableLocation = "Shippers", FilterContext = "Shippers[ShipperID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_27_CC_Hard_DE", TableLocation = "Mitarbeiter", FilterContext = "Mitarbeiter[MitarbeiterID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_27_CC_Hard_EN", TableLocation = "Employees", FilterContext = "Employees[EmployeeID] = \"1\"" },
            new PromptData { Prompt_ID = "Prompt_28_CC_Hard_DE", TableLocation = "Kunden", FilterContext = "Kunden[KundenID] = \"ANATR\"" },
            new PromptData { Prompt_ID = "Prompt_28_CC_Hard_EN", TableLocation = "Customers", FilterContext = "Customers[CustomerID] = \"ANATR\"" },
            new PromptData { Prompt_ID = "Prompt_29_CC_Hard_DE", TableLocation = "Lieferanten", FilterContext = "Lieferanten[LieferantenID] = \"4\"" },
            new PromptData { Prompt_ID = "Prompt_29_CC_Hard_EN", TableLocation = "Suppliers", FilterContext = "Suppliers[SupplierID] = \"4\"" },
            new PromptData { Prompt_ID = "Prompt_30_CC_Hard_DE", TableLocation = "Kunden", FilterContext = "Kunden[KundenID] = \"ALFKI\"" },
            new PromptData { Prompt_ID = "Prompt_30_CC_Hard_EN", TableLocation = "Customers", FilterContext = "Customers[CustomerID] = \"ALFKI\"" }
        };
    }

    public string GetDesiredValueOf(string promptId, string theDesiredColumn)
    {
        foreach (var prompt in Prompt_typeCC_Answers)
        {
            if (prompt.Prompt_ID == promptId)
            {
                switch (theDesiredColumn)
                {
                    case "TableLocation":
                        return prompt.TableLocation;
                    case "FilterContext":
                        return prompt.FilterContext;
                    default:
                        return "Invalid column name";
                }
            }
        }
        return "No Column inside Function GetDesiredValueOf is found!";
    }
}

string GPT_Antworten_Pfad = @"PFAD_STRING";
string Auswirtung_Ordner_Pfad = @"PFAD_STRING";
string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
string txtFileName = "Report Final - Calculated Columns - EN - GPT-4 - Temp 0.4 - " + timestamp + ".txt"; // Muss stetig bearbeitet werden
string txtFilePath = System.IO.Path.Combine(Auswirtung_Ordner_Pfad, txtFileName);
string DateiInhalt="";
///// Kopfzeile der Auswertung
string GPT_Answer_ID = "";
string Prompt_ID_Nummer = "";
string Prompt_Category = "";
string Prompt_Level = "";
string Prompt_Language = "";
string Prompt_GPT_Model = "";
string Prompt_Temperature = "";
string Prompt_Run = "";
string Actual_Result = "";
string Desired_Result = "";
string EvaluationStatus_1 = ""; //Successful or Failed
string EvaluationStatus_2 = ""; // If Failed -> Syntax Error or Semantic Error
string ExceptionMessage = ""; // If there is any
string ErrorMessage = ""; // Only If Syntax Error 
string Comment = "";
/////
string Prompt_ID = ""; //Prompt_16_CC_Low_DE
bool isValidDAX_fromFile = false;
string extracted_DAX_fromFile = "";
string DAX_ToRetrieveDesiredResult = "";
string DAX_ToRetrieveActualResult = "";
var dataFetcher = new DataFetcher();
string TableLocation = "";
string FilterContext = "";
// Für die endgültige Nachricht an User
int SumTests = 0;
int SumResults_Successful = 0;
int SumResults_Failed = 0;
int SumResults_SyntaxError = 0;
int sumResults_SemanticError = 0;
int SumInvalidTest = 0;
string summary = "";
int timeToSleep = 3000; // in Miliseconds
using (var writer = new StreamWriter(txtFilePath, false, System.Text.Encoding.UTF8))
{
    //Kopfzeile der Auswertung
    writer.WriteLine("GPT_Answer_ID|Prompt_ID_Nummer|Prompt_Category|Prompt_Level|Prompt_Language|Prompt_GPT_Model|Prompt_Temperature|Prompt_Run|Actual_Result|Desired_Result|EvaluationStatus_1|EvaluationStatus_2|ErrorMessage|ExceptionMessage|Comment");
    foreach (string file in Directory.EnumerateFiles(GPT_Antworten_Pfad, "*.txt"))
    {
        DateiInhalt = Regex.Replace(File.ReadAllText(file), @"\t|\n|\r|\u2028|\u2029", " ").Trim();
        int index = DateiInhalt.IndexOf("=");
        GPT_Answer_ID = Path.GetFileNameWithoutExtension(file);
        string[] parts = GPT_Answer_ID.Split('_');
        Prompt_ID_Nummer = parts[1];
        Prompt_Category = parts[2];
        Prompt_Level = parts[3];
        Prompt_Language = parts[4];
        Prompt_GPT_Model = parts[5];
        Prompt_Temperature = "\"" + parts[7] + "\"";
        Prompt_Run = parts[9];
        Prompt_ID = string.Join("_", parts.Take(5));
        // Prüfe ob ein Datei gefunden
        if (index >= 0)
        {
            extracted_DAX_fromFile = Regex.Replace(DateiInhalt.Substring(index + 1), @"\t|\n|\r|\u2028|\u2029", " ").Trim();
            isValidDAX_fromFile = true;
        }
        else
        {
            Comment = "Achtung: DAX Expression nicht gefunden oder ungültig. ---- Datei: " + GPT_Answer_ID + " ---- Inhalt: " + DateiInhalt;
            isValidDAX_fromFile = false;
        }
        // auf meine Lösungen zugreifen
        TableLocation = dataFetcher.GetDesiredValueOf(Prompt_ID, "TableLocation"); // Beispiel: Customers
        FilterContext = dataFetcher.GetDesiredValueOf(Prompt_ID, "FilterContext"); // Beispiel: Customers[CustomerID] = "ALFKI"
        if (isValidDAX_fromFile)
        {
            SumTests++;
            try
            {
                Model.Tables[TableLocation].AddCalculatedColumn(GPT_Answer_ID, extracted_DAX_fromFile);
                Model.Database.TOMDatabase.Model.RequestRefresh(ToM.RefreshType.Calculate);
                Model.Database.TOMDatabase.Model.SaveChanges();
                Thread.Sleep(timeToSleep);
                if (!string.IsNullOrEmpty(Model.Tables[TableLocation].Columns.FindByName(GPT_Answer_ID).ErrorMessage)) // When ErrorMessage field is not empty, meaning there is a Syntax Error.
                {
                    EvaluationStatus_1 = "Failed";
                    EvaluationStatus_2 = "Syntax Error";
                    ErrorMessage = Convert.ToString(Model.Tables[TableLocation].Columns.FindByName(GPT_Answer_ID).ErrorMessage);
                    ErrorMessage = Regex.Replace(ErrorMessage, @"\t|\n|\r|\u2028|\u2029", " ").Trim();
                    Model.Tables[TableLocation].Columns.FindByName(GPT_Answer_ID).Delete();
                    Model.Database.TOMDatabase.Model.RequestRefresh(ToM.RefreshType.Calculate);
                    Model.Database.TOMDatabase.Model.SaveChanges();
                    Thread.Sleep(timeToSleep);
                    SumResults_SyntaxError++;
                    SumResults_Failed++;
                }
                else
                {
                    DAX_ToRetrieveDesiredResult = "CALCULATE(MAX(" + TableLocation + "[" + Prompt_ID + "])," + FilterContext + ")"; // CALCULATE (MAX ( Customers[Prompt_16_CC_Low_EN] ), Customers[CustomerID] = "ALFKI")
                    DAX_ToRetrieveActualResult = "CALCULATE(MAX(" + TableLocation + "[" + GPT_Answer_ID + "])," + FilterContext + ")";
                    try
                    {
                        Desired_Result = Convert.ToString(TabularEditor.Scripting.ScriptHelper.EvaluateDax(DAX_ToRetrieveDesiredResult));
                        Actual_Result = Convert.ToString(TabularEditor.Scripting.ScriptHelper.EvaluateDax(DAX_ToRetrieveActualResult));
                        if (Desired_Result == Actual_Result)
                        {
                            EvaluationStatus_1 = "Successful";
                            EvaluationStatus_2 = "Successful";
                            SumResults_Successful++;
                        }
                        else
                        {
                            EvaluationStatus_1 = "Failed";
                            EvaluationStatus_2 = "Semantic Error";
                            sumResults_SemanticError++;
                            SumResults_Failed++;
                        }
                        Model.Tables[TableLocation].Columns.FindByName(GPT_Answer_ID).Delete();
                        Model.Database.TOMDatabase.Model.RequestRefresh(ToM.RefreshType.Calculate);
                        Model.Database.TOMDatabase.Model.SaveChanges();
                        Thread.Sleep(timeToSleep);
                    }
                    catch (Exception exp)
                    {
                        ExceptionMessage = "There is an Error while evaluating daxExpression_Semantic.---- Prompt_ID: " + GPT_Answer_ID + "--- daxExpresion_ToEvaluateSemantic:---- " + DAX_ToRetrieveDesiredResult + "---- " + exp.Message;
                        ExceptionMessage = Regex.Replace(ExceptionMessage, @"\t|\n|\r|\u2028|\u2029", " ").Trim();
                        SumInvalidTest++;
                    }
                }
            }
            catch (Exception exp)
            {
                ExceptionMessage = "Exception Name: " + exp.GetType().Name + "---- Exception Message: " + exp.Message;
                ExceptionMessage = Regex.Replace(ExceptionMessage, @"\t|\n|\r|\u2028|\u2029", " ").Trim();
                SumInvalidTest++;
            }
        }
        else
        {
            Comment = "Invalid extracted DAX Expression from file";
            SumInvalidTest++;
        }
        writer.WriteLine(GPT_Answer_ID + "|" + Prompt_ID_Nummer + "|" + Prompt_Category + "|" + Prompt_Level + "|" + Prompt_Language + "|" + Prompt_GPT_Model + "|" + Prompt_Temperature + "|" + Prompt_Run + "|" + Actual_Result + "|" + Desired_Result + "|" + EvaluationStatus_1 + "|" + EvaluationStatus_2 + "|" + ErrorMessage + "|" + ExceptionMessage + "|" + Comment);
        //für die neue Iteration vorbereiten
        GPT_Answer_ID = "";
        Prompt_ID_Nummer = "";
        Prompt_Category = "";
        Prompt_Level = "";
        Prompt_Language = "";
        Prompt_GPT_Model = "";
        Prompt_Temperature = "";
        Prompt_Run = "";
        Actual_Result = "";
        Desired_Result = "";
        EvaluationStatus_1 = ""; //Successful or Failed
        EvaluationStatus_2 = ""; // If Failed -> Syntax Error or Semantic Error
        ExceptionMessage = ""; // If there is any
        ErrorMessage = ""; // Only If Syntax Error 
        Comment = "";
        Prompt_ID = ""; //Prompt_16_CC_Low_DE
        isValidDAX_fromFile = false;
        extracted_DAX_fromFile = "";
        DAX_ToRetrieveDesiredResult = "";
        DAX_ToRetrieveActualResult = "";
        TableLocation = "";
        FilterContext = "";
    }
}
stopWatch.Stop();
// Get the elapsed time as a TimeSpan value.
TimeSpan ts = stopWatch.Elapsed;
// Format and display the TimeSpan value.
string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
    ts.Hours, ts.Minutes, ts.Seconds,
    ts.Milliseconds / 10);
summary = "The testing process completed successfully.\n\n" +
            "Report: " + txtFileName + "\n\n" +
            "Total Tests: " + SumTests.ToString() + "\n" +
            "Invalid Tests: " + SumInvalidTest.ToString() + "\n" +
            "Successful Tests: " + SumResults_Successful.ToString() + "\n" +
            "Failed Tests: " + SumResults_Failed.ToString() + "\n\n" +
            "-> Syntax Errors: " + SumResults_SyntaxError.ToString() + "\n" +
            "-> Semantic Errors: " + sumResults_SemanticError.ToString() + "\n\n\n\n" +
            "Time taken (HH:MM:SS:MS):  " + elapsedTime;
Info(summary);