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
string GPT_Antworten_Pfad = @"C:\Users\³\Desktop\Bachelorarbeit\2. Testumgebung\5. Prompts - Aufgabenstellungen\ALLE GPT Antworten\Antworten_Final_GPT-4_CalculatedTables_EN_Temp4";
string Auswirtung_Ordner_Pfad = @"C:\Users\³\Desktop\Bachelorarbeit\2. Testumgebung\5. Prompts - Aufgabenstellungen\99. Bewertungen";
string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
string txtFileName = "Final Report - Calculated Tables - EN - GPT-4 - Temp 0.4 - " + timestamp + ".txt";
string txtFilePath = System.IO.Path.Combine(Auswirtung_Ordner_Pfad, txtFileName);
string DateiInhalt = "";
///// Kopfzeile der Auswertung
string GPT_Answer_ID = "";
string Prompt_ID_Nummer = "";
string Prompt_Category = "";
string Prompt_Level = "";
string Prompt_Language = "";
string Prompt_GPT_Model = "";
string Prompt_Temperature = "";
string Prompt_Run = "";
string Actual_Result_RowCount = "";
string Actual_Result_ColumnCount = "";
string Desired_Result_RowCount = "";
string Desired_Result_ColumnCount = "";
string EvaluationStatus_1 = ""; //Successful or Failed
string EvaluationStatus_2 = ""; // If Failed -> Syntax Error or Semantic Error
string ExceptionMessage = ""; // If there is any
string ErrorMessage = ""; // Only If Syntax Error 
string Comment = "";
/////
string Prompt_ID = ""; //Prompt_16_CC_Low_DE
bool isValidDAX_fromFile = false;
string extracted_DAX_fromFile = "";
string DAX_TOPN_formated = "";
string DAX_ToReturn_RowCount = "";
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
    writer.WriteLine("GPT_Answer_ID|Prompt_ID_Nummer|Prompt_Category|Prompt_Level|Prompt_Language|Prompt_GPT_Model|Prompt_Temperature|Prompt_Run|Actual_Result_RowCount|Desired_Result_RowCount|Actual_Result_ColumnCount|Desired_Result_ColumnCount|EvaluationStatus_1|EvaluationStatus_2|ErrorMessage|ExceptionMessage|Comment");
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
        if (isValidDAX_fromFile)
        {
            SumTests++;
            try
            {
                DAX_ToReturn_RowCount = "var NewTable = (" + extracted_DAX_fromFile + ") var CountRows_= COUNTROWS(NewTable) Return CountRows_";
                Actual_Result_RowCount = Convert.ToString(TabularEditor.Scripting.ScriptHelper.EvaluateDax(DAX_ToReturn_RowCount));
                DAX_ToReturn_RowCount = "COUNTROWS(" + Prompt_ID + ")";
                Desired_Result_RowCount = Convert.ToString(TabularEditor.Scripting.ScriptHelper.EvaluateDax(DAX_ToReturn_RowCount));
                DAX_TOPN_formated = "TOPN(0, (" + extracted_DAX_fromFile + "))";
                try
                {
                    Model.AddCalculatedTable(GPT_Answer_ID, DAX_TOPN_formated);
                    Model.Database.TOMDatabase.Model.RequestRefresh(ToM.RefreshType.Calculate);
                    Model.Database.TOMDatabase.Model.SaveChanges();
                    Thread.Sleep(timeToSleep);
                    Actual_Result_ColumnCount = Convert.ToString(Model.Tables[GPT_Answer_ID].Columns.Count);
                    Desired_Result_ColumnCount = Convert.ToString(Model.Tables[Prompt_ID].Columns.Count);
                    if ((Actual_Result_RowCount == Desired_Result_RowCount) && (Actual_Result_ColumnCount == Desired_Result_ColumnCount)) // When ErrorMessage field is not empty, meaning there is a Syntax Error.
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
                }
                catch (Exception exp)
                {
                    ExceptionMessage = "Exception Name: " + exp.GetType().Name + "---- Exception Message: " + exp.Message;
                    ExceptionMessage = Regex.Replace(ExceptionMessage, @"\t|\n|\r|\u2028|\u2029", " ").Trim();
                }
            }
            catch (Exception exp)
            {
                EvaluationStatus_1 = "Failed";
                EvaluationStatus_2 = "Syntax Error";
                ErrorMessage = Convert.ToString(exp.Message);
                ErrorMessage = Regex.Replace(ErrorMessage, @"\t|\n|\r|\u2028|\u2029", " ").Trim();
                SumResults_SyntaxError++;
                SumResults_Failed++;
            }
        }
        else
        {
            Comment = "Invalid extracted DAX Expression from file";
            SumInvalidTest++;
        }
        writer.WriteLine(GPT_Answer_ID + "|" + Prompt_ID_Nummer + "|" + Prompt_Category + "|" + Prompt_Level + "|" + Prompt_Language + "|" + Prompt_GPT_Model + "|" + Prompt_Temperature + "|" + Prompt_Run + "|" + Actual_Result_RowCount + "|" + Desired_Result_RowCount + "|" + Actual_Result_ColumnCount + "|" + Desired_Result_ColumnCount + "|" + EvaluationStatus_1 + "|" + EvaluationStatus_2 + "|" + ErrorMessage + "|" + ExceptionMessage + "|" + Comment);
        //für die neue Iteration vorbereiten
        GPT_Answer_ID = "";
        Prompt_ID_Nummer = "";
        Prompt_Category = "";
        Prompt_Level = "";
        Prompt_Language = "";
        Prompt_GPT_Model = "";
        Prompt_Temperature = "";
        Prompt_Run = "";
        Actual_Result_RowCount = "";
        Desired_Result_RowCount = "";
        Actual_Result_ColumnCount = "";
        Desired_Result_ColumnCount = "";
        EvaluationStatus_1 = ""; //Successful or Failed
        EvaluationStatus_2 = ""; // If Failed -> Syntax Error or Semantic Error
        ExceptionMessage = ""; // If there is any
        ErrorMessage = ""; // Only If Syntax Error 
        Comment = "";
        Prompt_ID = ""; //Prompt_16_CC_Low_DE
        isValidDAX_fromFile = false;
        extracted_DAX_fromFile = "";
        DAX_TOPN_formated = "";
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

