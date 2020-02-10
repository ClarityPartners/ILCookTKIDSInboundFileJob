using JobProcessingInterface;
using MSXML3;
using System;
using System.IO;
using Tyler.Odyssey.JobProcessing;
using Tyler.Odyssey.Utils;
using ILCookTKIDSInboundFileJob.Helpers;
using System.Linq;
using ILCookTKIDSInboundFileJob.Exceptions;
using System.Xml;
using System.Xml.Linq;
using System.Runtime.Serialization.Json;
using Tyler.Odyssey.API.JobTemplate;
using Tyler.Integration.Framework;
using System.Xml.Serialization;
using Tyler.Odyssey.API.Shared;
using Tyler.Integration.General;

namespace ILCookTKIDSInboundFileJob
{
  internal class DataProcessor : TaskProcessor
  {
    // Constructor
    public DataProcessor(string SiteID, string JobTaskXML) : base(SiteID, JobTaskXML)
    {
      Logger.WriteToLog("JobTaskXML:\r\n" + JobTaskXML, LogLevel.Basic);

      // New up the context object
      Context = new Context(Logger);

      Logger.WriteToLog("Completed instantiation of context object", LogLevel.Verbose);

      // Retrieve the parameters for the job (which flags to add/remove)
      Context.DeriveParametersFromJobTaskXML(SiteID, JobTaskXML);
      Context.ValidateParameters();

      Logger.WriteToLog("Finished deriving parameters", LogLevel.Verbose);

      // TODO:  Add the code tables that need to be updated to the following function (Context.AddCacheItems())
      Context.AddCacheItems();
      Context.UpdateCache();

      Logger.WriteToLog("Completed cache update.", LogLevel.Verbose);
    }

    // Static constructor
    static DataProcessor()
    {
      Logger = new UtilsLogger(LogManager);
      Logger.WriteToLog("Logger Instantiated", LogLevel.Basic);
    }

    // Destructor
    ~DataProcessor()
    {
      Logger.WriteToLog("Disposing!", LogLevel.Basic);

      if (Context != null)
        Context.Dispose();
    }

    public static IUtilsLogManager LogManager = new UtilsLogManagerBase(Constants.LOG_REGISTRY_KEY);
    public static readonly UtilsLogger Logger;

    public IXMLDOMDocument TaskDocument { get; set; }

    internal Context Context { get; set; }

    public ITYLJobTaskUtility TaskUtility { get; set; }

    private object taskParms;
    public object TaskParms { get { return taskParms; } set { taskParms = value; } }

    public override void Run()
    {
      Logger.WriteToLog("Beginning Run Method", LogLevel.Basic);

      // TODO: Update API Processing Logic
      try
      {
        string fileName = "";
        foreach (string name in Directory.GetFiles(Context.Parameters.InputFilePath, "*.txt"))
        {
          Logger.WriteToLog("Made it in file for each loop", LogLevel.Basic);
          ProcessFile(name);
          fileName = name;
        }
      }
      catch (Exception e)
      {
        Context.Errors.Add(new BaseCustomException(e.Message));
      }

      // TODO: Handle errors we've collected during the job run.
      if (Context.Errors.Count > 0)
      {
        // Add a message to the job indicating that something went wrong.
        AddInformationToJob();

        // Collect errors, write them to a file, and attach the file to the job.
        LogErrors();
      }

      ContinueWithProcessing("Job Completed Successfully");
    }

    public void ProcessFile(String FileName)
    {
      using (StreamReader reader = new StreamReader(FileName))
      {
        Logger.WriteToLog("Processing File: " + FileName, LogLevel.Basic);

        // File mapping and extraction
        while (!reader.EndOfStream)
        {
          string line = reader.ReadLine();
          Logger.WriteToLog("this is the line = " + line, LogLevel.Basic);
          TKIDSData data = new TKIDSData();
          FileParser FP = new FileParser(line);
          FP.Read(data);
          data.cleanData();
          Logger.WriteToLog("Cleaned Data", LogLevel.Basic);
          Logger.WriteToLog("Event Code = " + data.EventCode, LogLevel.Basic);
          Logger.WriteToLog("Case Number = " + data.CaseNumber, LogLevel.Basic);
          Logger.WriteToLog("First Name = " + data.FirstName, LogLevel.Basic);
          Logger.WriteToLog("Last Name = " + data.LastName, LogLevel.Basic);
          Logger.WriteToLog("Other Agency Number = " + data.OtherAgencyNumber, LogLevel.Basic);
          String CaseID = "";
          //Change Party Other Number
          if (data.EventCode.Equals("012"))
          {
            CaseID = FindCase(data.CaseNumber);
            Entities.LoadCaseResult Case = LoadCase(CaseID);
            foreach (Entities.LoadCaseCaseParty Party in Case.Case.CaseParties.CaseParty) {
              Logger.WriteToLog("In For Loop", LogLevel.Basic);
              Entities.JusticePartyPerson person = Party.Names.PrimaryName.Item as Entities.JusticePartyPerson;
              if (person.First.Equals(data.FirstName) && person.Last.Equals(data.LastName)) {
                Logger.WriteToLog("Found a matching party", LogLevel.Basic);
                Entities.LoadPartyResultEntity LoadPartyResult = LoadParty(Party.PartyID);
                Entities.LOADPARTYOTHERAGENCYNUMBER[] OtherAgencyNumbers = LoadPartyResult.Party.OtherAgencyNumbers;
                DeletePartyDetails(Party.PartyID);
                Logger.WriteToLog("Before Match Update Party", LogLevel.Basic);
                MatchUpdateParty(Party.PartyID, data.OtherAgencyNumber, OtherAgencyNumbers);
              }
            }
          }
          //Change Case Cross Reference Number
          else if (data.EventCode.Equals("2"))
          {
            CaseID = FindCase(data.CaseNumber);
            Entities.LoadCaseResult Case = LoadCase(CaseID);
            foreach (Entities.LoadCaseCaseCrossReference number in Case.Case.CaseCrossReferences.Where(x => x.CrossReferenceNumberType.Equals("RR-RIN"))) {
              Logger.WriteToLog("Found a Case Cross Reference Number to replace", LogLevel.Basic);
              DeleteCaseCrossReferenceNumber(Case.Case.NodeID, CaseID, number);
              AddCaseCrossReference(Case.Case.NodeID, CaseID, data.OtherAgencyNumber);
            }
          }
          //Change Party Name -- Do not do this one yet --
          else if (data.EventCode.Equals("3"))
          {

          }
        }
      }
    }

    private Entities.LoadPartyResultEntity LoadParty(String PartyID) {
      Entities.LoadPartyEntity entity = new Entities.LoadPartyEntity();
      entity.NodeID = Entities.LOADPARTYNODEID.Item0;
      entity.ReferenceNumber = "LoadParty";
      entity.Source = "LoadParty";
      entity.UserID = "1";

      entity.PartyID = PartyID;
      entity.LoadEntities = new Entities.LOADPARTYLOADENTITIES();
      entity.LoadEntities.OtherAgencyNumber = "true";

      entity.MaxNumberOfResults = "1";
      entity.CurrentKnownOnly = "true";

      Logger.WriteToLog("LoadParty Message = " + entity.ToOdysseyMessageXml(), LogLevel.Basic);
      OdysseyMessage msg = new OdysseyMessage(entity.ToOdysseyMessageXml(), Context.SiteID);
      MessageHandlerFactory.Instance.ProcessMessage(msg);

      StringReader reader = new StringReader(msg.ResponseDocument.OuterXml);
      XmlSerializer serializer = new XmlSerializer(typeof(Entities.LoadPartyResultEntity));
      Entities.LoadPartyResultEntity result = (Entities.LoadPartyResultEntity)serializer.Deserialize(reader);

      return result;
    }

    private Entities.DeletePartyDetailsResultEntity DeletePartyDetails(String PartyID)
    {
      Entities.DeletePartyDetailsEntity entity = new Entities.DeletePartyDetailsEntity();
      //entity.SetStandardAttributes(int.Parse(Context.Parameters.NodeID), "AddCaseCrossReference", Context.UserID, "AddCaseCrossReference", Context.SiteID);
      entity.NodeID = Entities.BASEREQUIREDZERO.Item0;
      entity.ReferenceNumber = "DeletePartyDetails";
      entity.Source = "DeletePartyDetails";
      entity.UserID = "1";

      entity.PartyID = PartyID;
      entity.Delete = new Entities.DELETEPARTYDETAILSDELETE();
      entity.Delete.OtherAgencyNumbers = " ";

      Logger.WriteToLog("DeletePartyDetails Message = " + entity.ToOdysseyMessageXml(), LogLevel.Basic);
      OdysseyMessage msg = new OdysseyMessage(entity.ToOdysseyMessageXml(), Context.SiteID);
      MessageHandlerFactory.Instance.ProcessMessage(msg);

      StringReader reader = new StringReader(msg.ResponseDocument.OuterXml);
      XmlSerializer serializer = new XmlSerializer(typeof(Entities.DeletePartyDetailsResultEntity));
      Entities.DeletePartyDetailsResultEntity result = (Entities.DeletePartyDetailsResultEntity)serializer.Deserialize(reader);

      Logger.WriteToLog("At the end of the Delete Party Details", LogLevel.Basic);

      return result;
    }

    private Entities.AddCaseCrossReferenceResultEntity AddCaseCrossReference(String NodeID, String CaseID, String CrossReferenceNumber)
    {
      Entities.AddCaseCrossReferenceEntity entity = new Entities.AddCaseCrossReferenceEntity();
      //entity.SetStandardAttributes(int.Parse(Context.Parameters.NodeID), "AddCaseCrossReference", Context.UserID, "AddCaseCrossReference", Context.SiteID);
      entity.NodeID = NodeID;
      entity.ReferenceNumber = "AddCaseCrossReference";
      entity.Source = "AddCaseCrossReference";
      entity.UserID = "1";

      entity.CrossReferenceNumberType = "RR-RIN";
      entity.CrossReferenceNumber = CrossReferenceNumber;
      entity.CaseID = CaseID;

      Logger.WriteToLog("AddCaseCrossReference Message = " + entity.ToOdysseyMessageXml(), LogLevel.Basic);
      OdysseyMessage msg = new OdysseyMessage(entity.ToOdysseyMessageXml(), Context.SiteID);
      MessageHandlerFactory.Instance.ProcessMessage(msg);

      StringReader reader = new StringReader(msg.ResponseDocument.OuterXml);
      XmlSerializer serializer = new XmlSerializer(typeof(Entities.AddCaseCrossReferenceResultEntity));
      Entities.AddCaseCrossReferenceResultEntity result = (Entities.AddCaseCrossReferenceResultEntity)serializer.Deserialize(reader);

      return result;
    }

    private Entities.DeleteCaseCrossReferenceNumberResultEntity DeleteCaseCrossReferenceNumber(String NodeID, String CaseID, Entities.LoadCaseCaseCrossReference CrossReference)
    {
      Entities.DeleteCaseCrossReferenceNumberEntity entity = new Entities.DeleteCaseCrossReferenceNumberEntity();
      //entity.SetStandardAttributes(int.Parse(Context.Parameters.NodeID), "DeleteCaseCrossReference", Context.UserID, "DeleteCaseCrossReference", Context.SiteID);
      entity.NodeID = NodeID;
      entity.ReferenceNumber = "DeleteCaseCrossReference";
      entity.Source = "DeleteCaseCrossReference";
      entity.UserID = "1";

      entity.CrossReferenceNumberType = CrossReference.CrossReferenceNumberType;
      entity.CrossReferenceNumber = CrossReference.CrossReferenceNumber;
      entity.CaseID = CaseID;

      Logger.WriteToLog("DeleteCaseCrossReference Message = " + entity.ToOdysseyMessageXml(), LogLevel.Basic);
      OdysseyMessage msg = new OdysseyMessage(entity.ToOdysseyMessageXml(), Context.SiteID);
      MessageHandlerFactory.Instance.ProcessMessage(msg);

      StringReader reader = new StringReader(msg.ResponseDocument.OuterXml);
      XmlSerializer serializer = new XmlSerializer(typeof(Entities.DeleteCaseCrossReferenceNumberResultEntity));
      Entities.DeleteCaseCrossReferenceNumberResultEntity result = (Entities.DeleteCaseCrossReferenceNumberResultEntity)serializer.Deserialize(reader);

      return result;
    }

    // Call with a single API
    private string FindCase(String CaseNumber)
    {
      Entities.FindCaseByCaseNumberEntity entity = new Entities.FindCaseByCaseNumberEntity();
      //entity.SetStandardAttributes(int.Parse(Context.Parameters.NodeID), "FindCase", Context.UserID, "FindCase", Context.SiteID);
      entity.NodeID = "1";
      entity.ReferenceNumber = "FindCase";
      entity.Source = "FindCase";
      entity.UserID = "1";

      entity.CaseNumber = CaseNumber;


      Logger.WriteToLog("FindCase Message = " + entity.ToOdysseyMessageXml(), LogLevel.Basic);
      OdysseyMessage msg = new OdysseyMessage(entity.ToOdysseyMessageXml(), Context.SiteID);
      MessageHandlerFactory.Instance.ProcessMessage(msg);

      StringReader reader = new StringReader(msg.ResponseDocument.OuterXml);
      XmlSerializer serializer = new XmlSerializer(typeof(Entities.FindCaseByCaseNumberResultEntity));
      Entities.FindCaseByCaseNumberResultEntity result = (Entities.FindCaseByCaseNumberResultEntity)serializer.Deserialize(reader);

      return result.CaseID;
    }

    private Entities.LoadCaseResult LoadCase(String CaseID)
    {

      Entities.LoadCaseEntity entity = new Entities.LoadCaseEntity();
      entity.NodeID = "200";
      entity.ReferenceNumber = "LoadCase";
      entity.Source = "LoadCase";
      entity.UserID = "1";

      entity.CaseID = CaseID;
      Entities.LoadEntitiesCollection loadEntities = new Entities.LoadEntitiesCollection();
      loadEntities.CaseParties = "true";
      loadEntities.Charges = "true";
      loadEntities.CaseCrossReferences = "true";
      entity.LoadEntities = loadEntities;

      Logger.WriteToLog("LoadCase Message = " + entity.ToOdysseyMessageXml(), LogLevel.Basic);
      OdysseyMessage msg = new OdysseyMessage(entity.ToOdysseyMessageXml(), Context.SiteID);
      MessageHandlerFactory.Instance.ProcessMessage(msg);

      StringReader reader = new StringReader(msg.ResponseDocument.OuterXml);
      XmlSerializer serializer = new XmlSerializer(typeof(Entities.LoadCaseResult));
      Entities.LoadCaseResult result = (Entities.LoadCaseResult)serializer.Deserialize(reader);

      return result;
    }

    private Entities.MatchUpdatePartyResult MatchUpdateParty(String PartyID, String OtherAgencyNumber, Entities.LOADPARTYOTHERAGENCYNUMBER[] OtherAgencyNumbers)
    {
      Entities.MatchUpdateParty entity = new Entities.MatchUpdateParty();
      entity.NodeID = "0";
      entity.ReferenceNumber = "MatchUpdateParty";
      entity.Source = "MatchUpdateParty";
      entity.UserID = "1";

      entity.PartyID = PartyID;
      Entities.JusticePartyOtherAgencyNumber OtherAgency = new Entities.JusticePartyOtherAgencyNumber();

      Logger.WriteToLog("Before agency information", LogLevel.Basic);
      OtherAgency.AgencyNumber = OtherAgencyNumber;
      OtherAgency.AgencyType = "DRCS";

      var match = OtherAgencyNumbers.Where(x => x.AgencyType.Equals("DRCS"));

      if (match == null)
      {
        entity.OtherAgencyNumbers = new Entities.JusticePartyOtherAgencyNumber[OtherAgencyNumbers.Length + 1];
      }
      else
      {
        entity.OtherAgencyNumbers = new Entities.JusticePartyOtherAgencyNumber[OtherAgencyNumbers.Length];
      }

      Logger.WriteToLog("Almost before printing", LogLevel.Basic);
      int i = 0;
      foreach (Entities.LOADPARTYOTHERAGENCYNUMBER Number in OtherAgencyNumbers) {
        entity.OtherAgencyNumbers[i] = new Entities.JusticePartyOtherAgencyNumber();
        if (Number.AgencyType.Equals("DRCS"))
        {
          entity.OtherAgencyNumbers[i].AgencyType = Number.AgencyType;
          entity.OtherAgencyNumbers[i].AgencyNumber = OtherAgencyNumber;
        }
        else
        {
          entity.OtherAgencyNumbers[i].AgencyType = Number.AgencyType;
          entity.OtherAgencyNumbers[i].AgencyNumber = Number.Number;
        }
        i++;
      }

      i++;
      if (match == null) {
        entity.OtherAgencyNumbers[i].AgencyNumber = OtherAgencyNumber;
        entity.OtherAgencyNumbers[i].AgencyType = "DRCS";
      }


      Logger.WriteToLog("MatchUpdateParty Message = " + entity.ToOdysseyMessageXml(), LogLevel.Basic);
      OdysseyMessage msg = new OdysseyMessage(entity.ToOdysseyMessageXml(), Context.SiteID);
      MessageHandlerFactory.Instance.ProcessMessage(msg);

      StringReader reader = new StringReader(msg.ResponseDocument.OuterXml);
      XmlSerializer serializer = new XmlSerializer(typeof(Entities.MatchUpdatePartyResult));
      Entities.MatchUpdatePartyResult result = (Entities.MatchUpdatePartyResult)serializer.Deserialize(reader);

      return result;
    }

    // Call with API Transaction
    private string AddCaseEvent(string caseID)
    {
      Tyler.Odyssey.API.JobTemplate.AddCaseEventEntity entity = new Tyler.Odyssey.API.JobTemplate.AddCaseEventEntity();
      entity.SetStandardAttributes(int.Parse(Context.Parameters.NodeID), "AddEvent", Context.UserID, "AddEvent", Context.SiteID);
      entity.CaseID = caseID;
      entity.CaseEventType = Context.Parameters.EventCode;
      entity.Date = DateTime.Today.ToShortDateString();

      TransactionEntity txn = new TransactionEntity();
      txn.TransactionType = "TylerAPIJobAddCaseEvent";
      txn.Messages.Add(entity);

      return ProcessTransaction(txn.ToOdysseyTransactionXML());
    }

    // Process Transaction
    public string ProcessTransaction(string transXml)
    {
      string txnResults = string.Empty;

      try
      {
        OdysseyTransaction txn = new OdysseyTransaction(0, transXml, Context.SiteID);
        TransactionProcessor txnProcessor = new TransactionProcessor();
        txnProcessor.ProcessTransaction(txn);

        if (txn.TransactionRejected)
          throw new Exception(txn.RejectReason);
        else
          if (txn.ResponseDocument != null)
          txnResults = txn.ResponseDocument.OuterXml;
      }
      // if a schema exceptions is thrown, then throw the new exception with the data from the schema error.
      catch (SchemaValidationException svex)
      {
        throw new Exception(svex.ReplacementStrings[0]);
      }
      catch (DataConversionException dcex)
      {
        // check for an xslCodeQuery exception type in the inner exception
        // so we can report a better, more descriptive error 
        if (dcex.InnerException.GetType().Equals(typeof(XslCodeQueryException)))
        {
          XslCodeQueryException xcqe = (XslCodeQueryException)dcex.InnerException;
          throw new Exception(xcqe.ReplacementStrings[0], dcex);
        }
        else
        {
          throw new Exception(dcex.Message);
        }

      }
      catch (Exception ex)
      {
        throw ex;
      }

      return txnResults;
    }


    private void AddInformationToJob()
    {
      int jobTaskID = 0;
      int jobProcessID = 0;

      if (Int32.TryParse(Context.taskID, out jobTaskID) && Int32.TryParse(Context.jobProcessID, out jobProcessID))
      {
        object Parms = new object[,] { { "SEVERITY" }, { "2" } };

        ITYLJobTaskUtility taskUtility = (JobProcessingInterface.ITYLJobTaskUtility)Activator.CreateInstance(Type.GetTypeFromProgID("Tyler.Odyssey.JobProcessing.TYLJobTaskUtility.cTask"));

        taskUtility.AddTextMessage(Context.SiteID, jobProcessID, jobTaskID, "The job completed successfully, but some cases were not processed. Please see the attached error file for a list of those cases and the errors associated with each. A list manager list containing the cases in error was also created.", ref Parms);
      }
    }


    private void LogErrors()
    {
      using (StreamWriter writer = GetTempFile())
      {
        Logger.WriteToLog("Beginning to write to temp file.", LogLevel.Intermediate);

        // Write the file header
        writer.WriteLine("CaseNumber,CaseID,CaseFlag,Error");

        // For each error, write some information.
        Context.Errors.ForEach((BaseCustomException f) => WriteErrorToLog(f, writer));

        Logger.WriteToLog("Finished writing to temp file.", LogLevel.Intermediate);

        AttachTempFileToJobOutput(writer, @"Add Remove Case Flags Action - Errors");
      }
    }


    private void WriteErrorToLog(BaseCustomException exception, StreamWriter writer)
    {
      writer.WriteLine(string.Format("\"{0}\"", exception.CustomMessage));
    }


    private StreamWriter GetTempFile()
    {
      if (TaskUtility == null)
        return null;

      string filePath = TaskUtility.GenerateFile(Context.SiteID, ref taskParms);
      StreamWriter fileWriter = new StreamWriter(filePath, true);

      Logger.WriteToLog("Created temp file at location: " + filePath, LogLevel.Basic);

      return fileWriter;
    }


    private void AttachTempFileToJobOutput(StreamWriter writer, string errorFileName)
    {
      Logger.WriteToLog("Begining AttachTempFileToJobOutput()", LogLevel.Intermediate);
      Logger.WriteToLog(writer == null ? "File is NULL" : "File is NOT NULL", LogLevel.Intermediate);

      if (writer != null && TaskUtility != null)
      {
        FileStream fileStream = writer.BaseStream as FileStream;
        string filePath = fileStream.Name;
        Logger.WriteToLog("File Path: " + filePath, LogLevel.Intermediate);

        writer.Close();

        if (filePath.Length > 0 && errorFileName.Length > 0)
          AttachFile(filePath, errorFileName);

        Logger.WriteToLog("Completed AttachTempFileToJobOutput()", LogLevel.Intermediate);
      }
    }


    private void AttachFile(string filepath, string filename)
    {
      DataProcessor.Logger.WriteToLog("Begin AttachFile()", Tyler.Odyssey.Utils.LogLevel.Intermediate);
      int nodeID = 0;
      int taskIDInt = 0;
      int jobProcessIDInt = 0;

      if (TaskUtility != null)
      {
        if (Int32.TryParse(Context.taskID, out taskIDInt) && Int32.TryParse(Context.jobProcessID, out jobProcessIDInt))
        {
          int documentID = TaskUtility.AddOutputDocument(this.siteKey, taskIDInt, jobProcessIDInt, -1, filepath, Context.UserID, nodeID, ref taskParms);

          if (documentID > 0)
          {
            TaskUtility.AddOutputParams(this.siteKey, taskIDInt, "TEXT", documentID, filename, TaskDocument, ref taskParms);

            TaskUtility.DeleteTempFile(filepath);

            this.OutputJobTaskXML = TaskDocument.documentElement.xml;
          }
        }
      }

      DataProcessor.Logger.WriteToLog("End Attach()", Tyler.Odyssey.Utils.LogLevel.Intermediate);
    }
  }
}