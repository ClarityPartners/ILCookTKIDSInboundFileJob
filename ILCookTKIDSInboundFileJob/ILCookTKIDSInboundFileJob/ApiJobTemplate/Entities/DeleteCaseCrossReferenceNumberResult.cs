﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ILCookTKIDSInboundFileJob.Entities
{
  using System.Xml.Serialization;
  using Tyler.Odyssey.API.Shared;
  // 
  // This source code was auto-generated by xsd, Version=4.6.1055.0.
  // 


  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", ElementName ="Result",IsNullable = false)]
  public partial class DeleteCaseCrossReferenceNumberResultEntity
  {

    private string successField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString")]
    public string Success
    {
      get
      {
        return this.successField;
      }
      set
      {
        this.successField = value;
      }
    }
  }
}