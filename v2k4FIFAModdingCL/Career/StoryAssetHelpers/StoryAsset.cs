using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace v2k4FIFAModdingCL.Career.StoryAssetHelpers
{



    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class StoryAsset
    {

        private StoryAssetNodes nodesField;

        private StoryAssetGroups groupsField;

        private StoryAssetConnections connectionsField;

        private string idField;

        private string nameField;

        private string deletedField;

        private string validField;

        /// <remarks/>
        public StoryAssetNodes nodes
        {
            get
            {
                return this.nodesField;
            }
            set
            {
                this.nodesField = value;
            }
        }

        /// <remarks/>
        public StoryAssetGroups groups
        {
            get
            {
                return this.groupsField;
            }
            set
            {
                this.groupsField = value;
            }
        }

        /// <remarks/>
        public StoryAssetConnections connections
        {
            get
            {
                return this.connectionsField;
            }
            set
            {
                this.connectionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string deleted
        {
            get
            {
                return this.deletedField;
            }
            set
            {
                this.deletedField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string valid
        {
            get
            {
                return this.validField;
            }
            set
            {
                this.validField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodes
    {

        private object[] itemsField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("condition-node", typeof(StoryAssetNodesConditionnode))]
        [System.Xml.Serialization.XmlElementAttribute("conversation-node", typeof(StoryAssetNodesConversationnode))]
        [System.Xml.Serialization.XmlElementAttribute("join-node", typeof(StoryAssetNodesJoinnode))]
        [System.Xml.Serialization.XmlElementAttribute("modify-morale-node", typeof(StoryAssetNodesModifymoralenode))]
        [System.Xml.Serialization.XmlElementAttribute("save-data-node", typeof(StoryAssetNodesSavedatanode))]
        [System.Xml.Serialization.XmlElementAttribute("select-data-node", typeof(StoryAssetNodesSelectdatanode))]
        [System.Xml.Serialization.XmlElementAttribute("state-node", typeof(StoryAssetNodesStatenode))]
        [System.Xml.Serialization.XmlElementAttribute("story-end-node", typeof(StoryAssetNodesStoryendnode))]
        [System.Xml.Serialization.XmlElementAttribute("story-start-node", typeof(StoryAssetNodesStorystartnode))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConditionnode
    {

        private StoryAssetNodesConditionnodeConditions conditionsField;

        private string idField;

        private ushort xField;

        private ushort yField;

        private string nodeTypeField;

        private string nodeNameField;

        /// <remarks/>
        public StoryAssetNodesConditionnodeConditions conditions
        {
            get
            {
                return this.conditionsField;
            }
            set
            {
                this.conditionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConditionnodeConditions
    {

        private StoryAssetNodesConditionnodeConditionsCondition conditionField;

        private string idField;

        /// <remarks/>
        public StoryAssetNodesConditionnodeConditionsCondition condition
        {
            get
            {
                return this.conditionField;
            }
            set
            {
                this.conditionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConditionnodeConditionsCondition
    {

        private string idField;

        private string variableIdField;

        private string comparisonField;

        private byte valueField;

        private string operatorField;

        private string parenthesisField;

        private byte parenthesisCountField;

        private string isSelectConditionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string VariableId
        {
            get
            {
                return this.variableIdField;
            }
            set
            {
                this.variableIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Comparison
        {
            get
            {
                return this.comparisonField;
            }
            set
            {
                this.comparisonField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Operator
        {
            get
            {
                return this.operatorField;
            }
            set
            {
                this.operatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Parenthesis
        {
            get
            {
                return this.parenthesisField;
            }
            set
            {
                this.parenthesisField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte ParenthesisCount
        {
            get
            {
                return this.parenthesisCountField;
            }
            set
            {
                this.parenthesisCountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IsSelectCondition
        {
            get
            {
                return this.isSelectConditionField;
            }
            set
            {
                this.isSelectConditionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnode
    {

        private StoryAssetNodesConversationnodeStringWithArgsPlayerMessage stringWithArgsPlayerMessageField;

        private StoryAssetNodesConversationnodeManagerAnswerList managerAnswerListField;

        private StoryAssetNodesConversationnodeSupportedevents supportedeventsField;

        private string idField;

        private ushort xField;

        private ushort yField;

        private string nodeTypeField;

        private string nodeNameField;

        private string playerField;

        private byte priorityField;

        /// <remarks/>
        public StoryAssetNodesConversationnodeStringWithArgsPlayerMessage stringWithArgsPlayerMessage
        {
            get
            {
                return this.stringWithArgsPlayerMessageField;
            }
            set
            {
                this.stringWithArgsPlayerMessageField = value;
            }
        }

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerList managerAnswerList
        {
            get
            {
                return this.managerAnswerListField;
            }
            set
            {
                this.managerAnswerListField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("supported-events")]
        public StoryAssetNodesConversationnodeSupportedevents supportedevents
        {
            get
            {
                return this.supportedeventsField;
            }
            set
            {
                this.supportedeventsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Player
        {
            get
            {
                return this.playerField;
            }
            set
            {
                this.playerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Priority
        {
            get
            {
                return this.priorityField;
            }
            set
            {
                this.priorityField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeStringWithArgsPlayerMessage
    {

        private StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringArguments stringArgumentsField;

        private StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringContent stringContentField;

        private string idField;

        private string stringIDField;

        private byte genderSupportField;

        private string genderSupportTeamId1Field;

        private string genderSupportTeamId2Field;

        /// <remarks/>
        public StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringArguments stringArguments
        {
            get
            {
                return this.stringArgumentsField;
            }
            set
            {
                this.stringArgumentsField = value;
            }
        }

        /// <remarks/>
        public StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringContent stringContent
        {
            get
            {
                return this.stringContentField;
            }
            set
            {
                this.stringContentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StringID
        {
            get
            {
                return this.stringIDField;
            }
            set
            {
                this.stringIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte GenderSupport
        {
            get
            {
                return this.genderSupportField;
            }
            set
            {
                this.genderSupportField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GenderSupportTeamId1
        {
            get
            {
                return this.genderSupportTeamId1Field;
            }
            set
            {
                this.genderSupportTeamId1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GenderSupportTeamId2
        {
            get
            {
                return this.genderSupportTeamId2Field;
            }
            set
            {
                this.genderSupportTeamId2Field = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringArguments
    {

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringContent
    {

        private StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringContentKeyValuePair[] keyValuePairField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("keyValuePair")]
        public StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringContentKeyValuePair[] keyValuePair
        {
            get
            {
                return this.keyValuePairField;
            }
            set
            {
                this.keyValuePairField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeStringWithArgsPlayerMessageStringContentKeyValuePair
    {

        private string idField;

        private string keyField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerList
    {

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswer[] managerAnswerField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("managerAnswer")]
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswer[] managerAnswer
        {
            get
            {
                return this.managerAnswerField;
            }
            set
            {
                this.managerAnswerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswer
    {

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShort stringWithArgsAnswerShortField;

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLong stringWithArgsAnswerLongField;

        private string idField;

        private byte answerTypeField;

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShort stringWithArgsAnswerShort
        {
            get
            {
                return this.stringWithArgsAnswerShortField;
            }
            set
            {
                this.stringWithArgsAnswerShortField = value;
            }
        }

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLong stringWithArgsAnswerLong
        {
            get
            {
                return this.stringWithArgsAnswerLongField;
            }
            set
            {
                this.stringWithArgsAnswerLongField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte AnswerType
        {
            get
            {
                return this.answerTypeField;
            }
            set
            {
                this.answerTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShort
    {

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringArguments stringArgumentsField;

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringContent stringContentField;

        private string idField;

        private string stringIDField;

        private byte genderSupportField;

        private string genderSupportTeamId1Field;

        private string genderSupportTeamId2Field;

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringArguments stringArguments
        {
            get
            {
                return this.stringArgumentsField;
            }
            set
            {
                this.stringArgumentsField = value;
            }
        }

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringContent stringContent
        {
            get
            {
                return this.stringContentField;
            }
            set
            {
                this.stringContentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StringID
        {
            get
            {
                return this.stringIDField;
            }
            set
            {
                this.stringIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte GenderSupport
        {
            get
            {
                return this.genderSupportField;
            }
            set
            {
                this.genderSupportField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GenderSupportTeamId1
        {
            get
            {
                return this.genderSupportTeamId1Field;
            }
            set
            {
                this.genderSupportTeamId1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GenderSupportTeamId2
        {
            get
            {
                return this.genderSupportTeamId2Field;
            }
            set
            {
                this.genderSupportTeamId2Field = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringArguments
    {

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringArgumentsStringArgument stringArgumentField;

        private string idField;

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringArgumentsStringArgument stringArgument
        {
            get
            {
                return this.stringArgumentField;
            }
            set
            {
                this.stringArgumentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringArgumentsStringArgument
    {

        private string idField;

        private string valueField;

        private byte formatField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringContent
    {

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringContentKeyValuePair[] keyValuePairField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("keyValuePair")]
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringContentKeyValuePair[] keyValuePair
        {
            get
            {
                return this.keyValuePairField;
            }
            set
            {
                this.keyValuePairField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerShortStringContentKeyValuePair
    {

        private string idField;

        private string keyField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLong
    {

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringArguments stringArgumentsField;

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringContent stringContentField;

        private string idField;

        private string stringIDField;

        private byte genderSupportField;

        private string genderSupportTeamId1Field;

        private string genderSupportTeamId2Field;

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringArguments stringArguments
        {
            get
            {
                return this.stringArgumentsField;
            }
            set
            {
                this.stringArgumentsField = value;
            }
        }

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringContent stringContent
        {
            get
            {
                return this.stringContentField;
            }
            set
            {
                this.stringContentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StringID
        {
            get
            {
                return this.stringIDField;
            }
            set
            {
                this.stringIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte GenderSupport
        {
            get
            {
                return this.genderSupportField;
            }
            set
            {
                this.genderSupportField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GenderSupportTeamId1
        {
            get
            {
                return this.genderSupportTeamId1Field;
            }
            set
            {
                this.genderSupportTeamId1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GenderSupportTeamId2
        {
            get
            {
                return this.genderSupportTeamId2Field;
            }
            set
            {
                this.genderSupportTeamId2Field = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringArguments
    {

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringArgumentsStringArgument stringArgumentField;

        private string idField;

        /// <remarks/>
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringArgumentsStringArgument stringArgument
        {
            get
            {
                return this.stringArgumentField;
            }
            set
            {
                this.stringArgumentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringArgumentsStringArgument
    {

        private string idField;

        private string valueField;

        private byte formatField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringContent
    {

        private StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringContentKeyValuePair[] keyValuePairField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("keyValuePair")]
        public StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringContentKeyValuePair[] keyValuePair
        {
            get
            {
                return this.keyValuePairField;
            }
            set
            {
                this.keyValuePairField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeManagerAnswerListManagerAnswerStringWithArgsAnswerLongStringContentKeyValuePair
    {

        private string idField;

        private string keyField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeSupportedevents
    {

        private StoryAssetNodesConversationnodeSupportedeventsString[] stringField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("string")]
        public StoryAssetNodesConversationnodeSupportedeventsString[] @string
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesConversationnodeSupportedeventsString
    {

        private string idField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesJoinnode
    {

        private string idField;

        private ushort xField;

        private short yField;

        private string nodeTypeField;

        private string nodeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public short Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesModifymoralenode
    {

        private StoryAssetNodesModifymoralenodeEmotionimpact emotionimpactField;

        private StoryAssetNodesModifymoralenodePersonalityimpact personalityimpactField;

        private string idField;

        private ushort xField;

        private ushort yField;

        private string nodeTypeField;

        private string nodeNameField;

        private string playerIdField;

        private byte decayPeriodField;

        private string moraleImpactTypeField;

        private string moraleTemplateTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("emotion-impact")]
        public StoryAssetNodesModifymoralenodeEmotionimpact emotionimpact
        {
            get
            {
                return this.emotionimpactField;
            }
            set
            {
                this.emotionimpactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("personality-impact")]
        public StoryAssetNodesModifymoralenodePersonalityimpact personalityimpact
        {
            get
            {
                return this.personalityimpactField;
            }
            set
            {
                this.personalityimpactField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PlayerId
        {
            get
            {
                return this.playerIdField;
            }
            set
            {
                this.playerIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte DecayPeriod
        {
            get
            {
                return this.decayPeriodField;
            }
            set
            {
                this.decayPeriodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MoraleImpactType
        {
            get
            {
                return this.moraleImpactTypeField;
            }
            set
            {
                this.moraleImpactTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string MoraleTemplateType
        {
            get
            {
                return this.moraleTemplateTypeField;
            }
            set
            {
                this.moraleTemplateTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesModifymoralenodeEmotionimpact
    {

        private StoryAssetNodesModifymoralenodeEmotionimpactKeyValuePair[] keyValuePairField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("keyValuePair")]
        public StoryAssetNodesModifymoralenodeEmotionimpactKeyValuePair[] keyValuePair
        {
            get
            {
                return this.keyValuePairField;
            }
            set
            {
                this.keyValuePairField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesModifymoralenodeEmotionimpactKeyValuePair
    {

        private string idField;

        private string keyField;

        private sbyte valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public sbyte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesModifymoralenodePersonalityimpact
    {

        private StoryAssetNodesModifymoralenodePersonalityimpactKeyValuePair[] keyValuePairField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("keyValuePair")]
        public StoryAssetNodesModifymoralenodePersonalityimpactKeyValuePair[] keyValuePair
        {
            get
            {
                return this.keyValuePairField;
            }
            set
            {
                this.keyValuePairField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesModifymoralenodePersonalityimpactKeyValuePair
    {

        private string idField;

        private string keyField;

        private sbyte valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public sbyte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSavedatanode
    {

        private StoryAssetNodesSavedatanodeSaveItems saveItemsField;

        private string idField;

        private ushort xField;

        private ushort yField;

        private string nodeTypeField;

        private string nodeNameField;

        /// <remarks/>
        public StoryAssetNodesSavedatanodeSaveItems saveItems
        {
            get
            {
                return this.saveItemsField;
            }
            set
            {
                this.saveItemsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSavedatanodeSaveItems
    {

        private StoryAssetNodesSavedatanodeSaveItemsSaveItem[] saveItemField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("saveItem")]
        public StoryAssetNodesSavedatanodeSaveItemsSaveItem[] saveItem
        {
            get
            {
                return this.saveItemField;
            }
            set
            {
                this.saveItemField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSavedatanodeSaveItemsSaveItem
    {

        private string idField;

        private string keyField;

        private string valueField;

        private string valueTypeField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ValueType
        {
            get
            {
                return this.valueTypeField;
            }
            set
            {
                this.valueTypeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSelectdatanode
    {

        private StoryAssetNodesSelectdatanodeSelectdata selectdataField;

        private string idField;

        private ushort xField;

        private ushort yField;

        private string nodeTypeField;

        private string nodeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("select-data")]
        public StoryAssetNodesSelectdatanodeSelectdata selectdata
        {
            get
            {
                return this.selectdataField;
            }
            set
            {
                this.selectdataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSelectdatanodeSelectdata
    {

        private StoryAssetNodesSelectdatanodeSelectdataAliaslist aliaslistField;

        private StoryAssetNodesSelectdatanodeSelectdataOrderby orderbyField;

        private StoryAssetNodesSelectdatanodeSelectdataAliasdatasource aliasdatasourceField;

        private string idField;

        private byte selectCountField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("alias-list")]
        public StoryAssetNodesSelectdatanodeSelectdataAliaslist aliaslist
        {
            get
            {
                return this.aliaslistField;
            }
            set
            {
                this.aliaslistField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("order-by")]
        public StoryAssetNodesSelectdatanodeSelectdataOrderby orderby
        {
            get
            {
                return this.orderbyField;
            }
            set
            {
                this.orderbyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("alias-data-source")]
        public StoryAssetNodesSelectdatanodeSelectdataAliasdatasource aliasdatasource
        {
            get
            {
                return this.aliasdatasourceField;
            }
            set
            {
                this.aliasdatasourceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte SelectCount
        {
            get
            {
                return this.selectCountField;
            }
            set
            {
                this.selectCountField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSelectdatanodeSelectdataAliaslist
    {

        private StoryAssetNodesSelectdatanodeSelectdataAliaslistString stringField;

        private string idField;

        /// <remarks/>
        public StoryAssetNodesSelectdatanodeSelectdataAliaslistString @string
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSelectdatanodeSelectdataAliaslistString
    {

        private string idField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSelectdatanodeSelectdataOrderby
    {

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSelectdatanodeSelectdataAliasdatasource
    {

        private StoryAssetNodesSelectdatanodeSelectdataAliasdatasourceConditions conditionsField;

        private string idField;

        private string dataSourceField;

        /// <remarks/>
        public StoryAssetNodesSelectdatanodeSelectdataAliasdatasourceConditions conditions
        {
            get
            {
                return this.conditionsField;
            }
            set
            {
                this.conditionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DataSource
        {
            get
            {
                return this.dataSourceField;
            }
            set
            {
                this.dataSourceField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSelectdatanodeSelectdataAliasdatasourceConditions
    {

        private StoryAssetNodesSelectdatanodeSelectdataAliasdatasourceConditionsCondition[] conditionField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("condition")]
        public StoryAssetNodesSelectdatanodeSelectdataAliasdatasourceConditionsCondition[] condition
        {
            get
            {
                return this.conditionField;
            }
            set
            {
                this.conditionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesSelectdatanodeSelectdataAliasdatasourceConditionsCondition
    {

        private string idField;

        private string variableIdField;

        private string comparisonField;

        private string valueField;

        private string operatorField;

        private string parenthesisField;

        private byte parenthesisCountField;

        private string isSelectConditionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string VariableId
        {
            get
            {
                return this.variableIdField;
            }
            set
            {
                this.variableIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Comparison
        {
            get
            {
                return this.comparisonField;
            }
            set
            {
                this.comparisonField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Operator
        {
            get
            {
                return this.operatorField;
            }
            set
            {
                this.operatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Parenthesis
        {
            get
            {
                return this.parenthesisField;
            }
            set
            {
                this.parenthesisField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte ParenthesisCount
        {
            get
            {
                return this.parenthesisCountField;
            }
            set
            {
                this.parenthesisCountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IsSelectCondition
        {
            get
            {
                return this.isSelectConditionField;
            }
            set
            {
                this.isSelectConditionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStatenode
    {

        private StoryAssetNodesStatenodeSupportedevents supportedeventsField;

        private string idField;

        private ushort xField;

        private ushort yField;

        private string nodeTypeField;

        private string nodeNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("supported-events")]
        public StoryAssetNodesStatenodeSupportedevents supportedevents
        {
            get
            {
                return this.supportedeventsField;
            }
            set
            {
                this.supportedeventsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStatenodeSupportedevents
    {

        private StoryAssetNodesStatenodeSupportedeventsString stringField;

        private string idField;

        /// <remarks/>
        public StoryAssetNodesStatenodeSupportedeventsString @string
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStatenodeSupportedeventsString
    {

        private string idField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStoryendnode
    {

        private string idField;

        private ushort xField;

        private ushort yField;

        private string nodeTypeField;

        private string nodeNameField;

        private string nodeEventField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ushort Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeEvent
        {
            get
            {
                return this.nodeEventField;
            }
            set
            {
                this.nodeEventField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnode
    {

        private StoryAssetNodesStorystartnodeConditions conditionsField;

        private StoryAssetNodesStorystartnodeSelectdata selectdataField;

        private string idField;

        private byte xField;

        private byte yField;

        private string nodeTypeField;

        private string nodeNameField;

        private string storyTypeField;

        private string nodeEventField;

        private string storyKeyField;

        private string useSelectDataField;

        private string runInPlayerCareerField;

        private string runInManagerCareerField;

        private string runInTournamentModeField;

        /// <remarks/>
        public StoryAssetNodesStorystartnodeConditions conditions
        {
            get
            {
                return this.conditionsField;
            }
            set
            {
                this.conditionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("select-data")]
        public StoryAssetNodesStorystartnodeSelectdata selectdata
        {
            get
            {
                return this.selectdataField;
            }
            set
            {
                this.selectdataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeType
        {
            get
            {
                return this.nodeTypeField;
            }
            set
            {
                this.nodeTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeName
        {
            get
            {
                return this.nodeNameField;
            }
            set
            {
                this.nodeNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StoryType
        {
            get
            {
                return this.storyTypeField;
            }
            set
            {
                this.storyTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string NodeEvent
        {
            get
            {
                return this.nodeEventField;
            }
            set
            {
                this.nodeEventField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string StoryKey
        {
            get
            {
                return this.storyKeyField;
            }
            set
            {
                this.storyKeyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string UseSelectData
        {
            get
            {
                return this.useSelectDataField;
            }
            set
            {
                this.useSelectDataField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RunInPlayerCareer
        {
            get
            {
                return this.runInPlayerCareerField;
            }
            set
            {
                this.runInPlayerCareerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RunInManagerCareer
        {
            get
            {
                return this.runInManagerCareerField;
            }
            set
            {
                this.runInManagerCareerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string RunInTournamentMode
        {
            get
            {
                return this.runInTournamentModeField;
            }
            set
            {
                this.runInTournamentModeField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeConditions
    {

        private StoryAssetNodesStorystartnodeConditionsCondition[] conditionField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("condition")]
        public StoryAssetNodesStorystartnodeConditionsCondition[] condition
        {
            get
            {
                return this.conditionField;
            }
            set
            {
                this.conditionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeConditionsCondition
    {

        private string idField;

        private string variableIdField;

        private string comparisonField;

        private byte valueField;

        private string operatorField;

        private string parenthesisField;

        private byte parenthesisCountField;

        private string isSelectConditionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string VariableId
        {
            get
            {
                return this.variableIdField;
            }
            set
            {
                this.variableIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Comparison
        {
            get
            {
                return this.comparisonField;
            }
            set
            {
                this.comparisonField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Operator
        {
            get
            {
                return this.operatorField;
            }
            set
            {
                this.operatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Parenthesis
        {
            get
            {
                return this.parenthesisField;
            }
            set
            {
                this.parenthesisField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte ParenthesisCount
        {
            get
            {
                return this.parenthesisCountField;
            }
            set
            {
                this.parenthesisCountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IsSelectCondition
        {
            get
            {
                return this.isSelectConditionField;
            }
            set
            {
                this.isSelectConditionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeSelectdata
    {

        private StoryAssetNodesStorystartnodeSelectdataAliaslist aliaslistField;

        private StoryAssetNodesStorystartnodeSelectdataOrderby orderbyField;

        private StoryAssetNodesStorystartnodeSelectdataAliasdatasource aliasdatasourceField;

        private string idField;

        private byte selectCountField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("alias-list")]
        public StoryAssetNodesStorystartnodeSelectdataAliaslist aliaslist
        {
            get
            {
                return this.aliaslistField;
            }
            set
            {
                this.aliaslistField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("order-by")]
        public StoryAssetNodesStorystartnodeSelectdataOrderby orderby
        {
            get
            {
                return this.orderbyField;
            }
            set
            {
                this.orderbyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("alias-data-source")]
        public StoryAssetNodesStorystartnodeSelectdataAliasdatasource aliasdatasource
        {
            get
            {
                return this.aliasdatasourceField;
            }
            set
            {
                this.aliasdatasourceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte SelectCount
        {
            get
            {
                return this.selectCountField;
            }
            set
            {
                this.selectCountField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeSelectdataAliaslist
    {

        private StoryAssetNodesStorystartnodeSelectdataAliaslistString stringField;

        private string idField;

        /// <remarks/>
        public StoryAssetNodesStorystartnodeSelectdataAliaslistString @string
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeSelectdataAliaslistString
    {

        private string idField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeSelectdataOrderby
    {

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeSelectdataAliasdatasource
    {

        private StoryAssetNodesStorystartnodeSelectdataAliasdatasourceConditions conditionsField;

        private string idField;

        private string dataSourceField;

        /// <remarks/>
        public StoryAssetNodesStorystartnodeSelectdataAliasdatasourceConditions conditions
        {
            get
            {
                return this.conditionsField;
            }
            set
            {
                this.conditionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DataSource
        {
            get
            {
                return this.dataSourceField;
            }
            set
            {
                this.dataSourceField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeSelectdataAliasdatasourceConditions
    {

        private StoryAssetNodesStorystartnodeSelectdataAliasdatasourceConditionsCondition[] conditionField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("condition")]
        public StoryAssetNodesStorystartnodeSelectdataAliasdatasourceConditionsCondition[] condition
        {
            get
            {
                return this.conditionField;
            }
            set
            {
                this.conditionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetNodesStorystartnodeSelectdataAliasdatasourceConditionsCondition
    {

        private string idField;

        private string variableIdField;

        private string comparisonField;

        private byte valueField;

        private string operatorField;

        private string parenthesisField;

        private byte parenthesisCountField;

        private string isSelectConditionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string VariableId
        {
            get
            {
                return this.variableIdField;
            }
            set
            {
                this.variableIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Comparison
        {
            get
            {
                return this.comparisonField;
            }
            set
            {
                this.comparisonField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Operator
        {
            get
            {
                return this.operatorField;
            }
            set
            {
                this.operatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Parenthesis
        {
            get
            {
                return this.parenthesisField;
            }
            set
            {
                this.parenthesisField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte ParenthesisCount
        {
            get
            {
                return this.parenthesisCountField;
            }
            set
            {
                this.parenthesisCountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string IsSelectCondition
        {
            get
            {
                return this.isSelectConditionField;
            }
            set
            {
                this.isSelectConditionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetGroups
    {

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetConnections
    {

        private StoryAssetConnectionsConnection[] connectionField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("connection")]
        public StoryAssetConnectionsConnection[] connection
        {
            get
            {
                return this.connectionField;
            }
            set
            {
                this.connectionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetConnectionsConnection
    {

        private StoryAssetConnectionsConnectionPoints pointsField;

        private string idField;

        private string srcNodeIDField;

        private string srcPortNameField;

        private string dstNodeIDField;

        private string dstPortNameField;

        private byte connectionOrderField;

        /// <remarks/>
        public StoryAssetConnectionsConnectionPoints points
        {
            get
            {
                return this.pointsField;
            }
            set
            {
                this.pointsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SrcNodeID
        {
            get
            {
                return this.srcNodeIDField;
            }
            set
            {
                this.srcNodeIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SrcPortName
        {
            get
            {
                return this.srcPortNameField;
            }
            set
            {
                this.srcPortNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DstNodeID
        {
            get
            {
                return this.dstNodeIDField;
            }
            set
            {
                this.dstNodeIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DstPortName
        {
            get
            {
                return this.dstPortNameField;
            }
            set
            {
                this.dstPortNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte ConnectionOrder
        {
            get
            {
                return this.connectionOrderField;
            }
            set
            {
                this.connectionOrderField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetConnectionsConnectionPoints
    {

        private StoryAssetConnectionsConnectionPointsPoint[] pointField;

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("point")]
        public StoryAssetConnectionsConnectionPointsPoint[] point
        {
            get
            {
                return this.pointField;
            }
            set
            {
                this.pointField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class StoryAssetConnectionsConnectionPointsPoint
    {

        private string idField;

        private decimal xField;

        private decimal yField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public decimal Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }
    }


}
