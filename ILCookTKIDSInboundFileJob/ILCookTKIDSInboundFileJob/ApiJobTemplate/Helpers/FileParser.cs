using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ILCookTKIDSInboundFileJob.Helpers
{
  class FileParser
  {
    private string line;

    public FileParser(string line)
    {
      this.line = line;
    }

    public TKIDSData Read(TKIDSData dataStructure)
    {
      foreach (FieldInfo field in typeof(TKIDSData).GetFields()) {
        foreach (object attr in field.GetCustomAttributes()) {
          if (attr is LayoutAttribute) {
            LayoutAttribute la = (LayoutAttribute)attr;
            string value = line.Substring(la.index, la.length);
            field.SetValue(dataStructure, value);
          }
        }

      }

      return dataStructure;
    }
  }
}
