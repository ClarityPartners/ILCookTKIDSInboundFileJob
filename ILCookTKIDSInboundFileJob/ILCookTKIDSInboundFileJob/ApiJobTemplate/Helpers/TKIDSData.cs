using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ILCookTKIDSInboundFileJob.Helpers
{
  class TKIDSData
  {
    [Layout(0, 3)]
    public string EventCode;

    [Layout(4, 6)]
    public string FirstName;

    [Layout(10, 10)]
    public string LastName;

    [Layout(20, 14)]
    public string CaseNumber;

    [Layout(36, 6)]
    public string OtherAgencyNumber;

    public void cleanData()
    {
      CaseNumber = CaseNumber.Trim();
      EventCode = EventCode.Trim();
      FirstName = FirstName.Trim();
      LastName = LastName.Trim();
      OtherAgencyNumber = OtherAgencyNumber.Trim();
    }
  }
}
