﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.6.81.0.
// 
namespace ILCookTKIDSInboundFileJob.Entities
{
  using System.Xml.Serialization;
  using Tyler.Odyssey.API.Shared;


  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
  [System.Xml.Serialization.XmlRootAttribute(Namespace = "", ElementName = "Result", IsNullable = false)]
  public partial class FindCaseByCaseNumberResultEntity : BaseGeneratedAPIEntity
  {

    private string caseIDField;

    private string caseNumberField;

    private string caseStyleField;

    private string caseStatusField;

    private string caseTypeField;

    private string caseSecurityGroupField;

    private LOCALCASECONSOLIDATION caseConsolidationField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
    public string NodeID
    {
      get
      {
        return this.nodeIDField;
      }
      set
      {
        this.nodeIDField = value;
      }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
    public string CaseID
    {
      get
      {
        return this.caseIDField;
      }
      set
      {
        this.caseIDField = value;
      }
    }

    /// <remarks/>
    public string CaseNumber
    {
      get
      {
        return this.caseNumberField;
      }
      set
      {
        this.caseNumberField = value;
      }
    }

    /// <remarks/>
    public string CaseStyle
    {
      get
      {
        return this.caseStyleField;
      }
      set
      {
        this.caseStyleField = value;
      }
    }

    /// <remarks/>
    public string CaseStatus
    {
      get
      {
        return this.caseStatusField;
      }
      set
      {
        this.caseStatusField = value;
      }
    }

    /// <remarks/>
    public string CaseType
    {
      get
      {
        return this.caseTypeField;
      }
      set
      {
        this.caseTypeField = value;
      }
    }

    /// <remarks/>
    public string CaseSecurityGroup
    {
      get
      {
        return this.caseSecurityGroupField;
      }
      set
      {
        this.caseSecurityGroupField = value;
      }
    }

    /// <remarks/>
    public LOCALCASECONSOLIDATION CaseConsolidation
    {
      get
      {
        return this.caseConsolidationField;
      }
      set
      {
        this.caseConsolidationField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(TypeName = "LOCAL.CASECONSOLIDATION")]
  public partial class LOCALCASECONSOLIDATION
  {

    private LOCALLEADCASE leadCaseField;

    /// <remarks/>
    public LOCALLEADCASE LeadCase
    {
      get
      {
        return this.leadCaseField;
      }
      set
      {
        this.leadCaseField = value;
      }
    }
  }

  /// <remarks/>
  [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
  [System.SerializableAttribute()]
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Xml.Serialization.XmlTypeAttribute(TypeName = "LOCAL.LEADCASE")]
  public partial class LOCALLEADCASE
  {

    private string caseIDField;

    private string caseNumberField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
    public string CaseID
    {
      get
      {
        return this.caseIDField;
      }
      set
      {
        this.caseIDField = value;
      }
    }

    /// <remarks/>
    public string CaseNumber
    {
      get
      {
        return this.caseNumberField;
      }
      set
      {
        this.caseNumberField = value;
      }
    }
  }
}
