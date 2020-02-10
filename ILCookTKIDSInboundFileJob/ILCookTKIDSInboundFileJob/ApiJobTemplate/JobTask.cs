using System;
using System.Runtime.InteropServices;
using Tyler.Odyssey.JobProcessing;

namespace ILCookTKIDSInboundFileJob
{
  [ClassInterface(ClassInterfaceType.None)]
  [Guid("745fd645-ddbd-42a5-9d20-e488e6d5de53")]
  [ComVisible(true)]
  public class JobTask : Task
  {
    protected override void SetupProcessor(string SiteID, string JobTaskXML)
    {
      Processor = new DataProcessor(SiteID, JobTaskXML);

      ((DataProcessor)Processor).TaskParms = this.jobTaskParms;
      ((DataProcessor)Processor).TaskUtility = this.taskUtility;
      ((DataProcessor)Processor).TaskDocument = this.taskDocument;

      UserID = ((DataProcessor)Processor).Context.UserID;
    }

    private int UserID { get; set; }
  }
}
