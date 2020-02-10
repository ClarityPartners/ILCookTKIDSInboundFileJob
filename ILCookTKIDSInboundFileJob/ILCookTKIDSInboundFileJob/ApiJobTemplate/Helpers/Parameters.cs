using System.Xml;
using Tyler.Odyssey.Utils;

namespace ILCookTKIDSInboundFileJob.Helpers
{
  public class Parameters
  {
    public string CaseNumber { get; private set; }
    public string NodeID { get; private set; }
    public string EventCode { get; private set; }
    public string InputFilePath { get; private set; }

    public Parameters(XmlElement taskNode, UtilsLogger logger)
    {
      logger.WriteToLog("Beginning Parameters() constructor", LogLevel.Verbose);
      logger.WriteToLog("taskNode: " + taskNode.OuterXml, LogLevel.Verbose);

      CaseNumber = taskNode.GetAttribute("CaseNumber");
      NodeID = taskNode.GetAttribute("NodeID");
      EventCode = taskNode.GetAttribute("EventCode");
      InputFilePath = taskNode.GetAttribute("InputFilePath");

      logger.WriteToLog("Instantiated Parameters", LogLevel.Verbose);
    }
  }
}