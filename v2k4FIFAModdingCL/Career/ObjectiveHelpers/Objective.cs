using System;
using System.Collections.Generic;
using System.Text;

namespace v2k4FIFAModdingCL.Career.ObjectiveHelpers
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class objectives
    {

        private objectivesObjective[] objectiveField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("objective")]
        public objectivesObjective[] objective
        {
            get
            {
                return this.objectiveField;
            }
            set
            {
                this.objectiveField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjective
    {

        private objectivesObjectiveDuration durationField;

        private objectivesObjectiveSimilarities similaritiesField;

        private ObjectiveConditions conditionsField;

        private objectivesObjectivePrecondition[] preconditionsField;

        private objectivesObjectiveExpected_progress expected_progressField;

        private string typeField;

        private byte scoreField;

        /// <remarks/>
        public objectivesObjectiveDuration duration
        {
            get
            {
                return this.durationField;
            }
            set
            {
                this.durationField = value;
            }
        }

        /// <remarks/>
        public objectivesObjectiveSimilarities similarities
        {
            get
            {
                return this.similaritiesField;
            }
            set
            {
                this.similaritiesField = value;
            }
        }

        /// <remarks/>
        public ObjectiveConditions conditions
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
        [System.Xml.Serialization.XmlArrayItemAttribute("precondition", IsNullable = false)]
        public objectivesObjectivePrecondition[] preconditions
        {
            get
            {
                return this.preconditionsField;
            }
            set
            {
                this.preconditionsField = value;
            }
        }

        /// <remarks/>
        public objectivesObjectiveExpected_progress expected_progress
        {
            get
            {
                return this.expected_progressField;
            }
            set
            {
                this.expected_progressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte score
        {
            get
            {
                return this.scoreField;
            }
            set
            {
                this.scoreField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjectiveDuration
    {

        private byte minField;

        private byte maxField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte min
        {
            get
            {
                return this.minField;
            }
            set
            {
                this.minField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte max
        {
            get
            {
                return this.maxField;
            }
            set
            {
                this.maxField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjectiveSimilarities
    {

        private objectivesObjectiveSimilaritiesSimilarity similarityField;

        /// <remarks/>
        public objectivesObjectiveSimilaritiesSimilarity similarity
        {
            get
            {
                return this.similarityField;
            }
            set
            {
                this.similarityField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjectiveSimilaritiesSimilarity
    {

        private string objectiveField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string objective
        {
            get
            {
                return this.objectiveField;
            }
            set
            {
                this.objectiveField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ObjectiveConditions
    {

        private objectivesObjectiveConditionsCondition[] conditionField;

        private byte non_mandatory_max_failField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("condition")]
        public objectivesObjectiveConditionsCondition[] condition
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
        public byte non_mandatory_max_fail
        {
            get
            {
                return this.non_mandatory_max_failField;
            }
            set
            {
                this.non_mandatory_max_failField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjectiveConditionsCondition
    {

        private objectivesObjectiveConditionsConditionArgument[] argumentField;

        private string typeField;

        private bool mandatoryField;

        private byte argumentsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("argument")]
        public objectivesObjectiveConditionsConditionArgument[] argument
        {
            get
            {
                return this.argumentField;
            }
            set
            {
                this.argumentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string offset
        {
            get;set;
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool mandatory
        {
            get
            {
                return this.mandatoryField;
            }
            set
            {
                this.mandatoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte arguments
        {
            get
            {
                return this.argumentsField;
            }
            set
            {
                this.argumentsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjectiveConditionsConditionArgument
    {

        private string nameField;

        private uint valueField;

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
        public uint value
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
    public partial class objectivesObjectivePrecondition
    {

        private objectivesObjectivePreconditionArgument[] argumentField;

        private string typeField;

        private byte argumentsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("argument")]
        public objectivesObjectivePreconditionArgument[] argument
        {
            get
            {
                return this.argumentField;
            }
            set
            {
                this.argumentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte arguments
        {
            get
            {
                return this.argumentsField;
            }
            set
            {
                this.argumentsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjectivePreconditionArgument
    {

        private string nameField;

        private uint valueField;

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
        public uint value
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
    public partial class objectivesObjectiveExpected_progress
    {

        private objectivesObjectiveExpected_progressProgress progressField;

        /// <remarks/>
        public objectivesObjectiveExpected_progressProgress progress
        {
            get
            {
                return this.progressField;
            }
            set
            {
                this.progressField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjectiveExpected_progressProgress
    {

        private objectivesObjectiveExpected_progressProgressEntry[] entryField;

        private string typeField;

        private byte durationField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("entry")]
        public objectivesObjectiveExpected_progressProgressEntry[] entry
        {
            get
            {
                return this.entryField;
            }
            set
            {
                this.entryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte duration
        {
            get
            {
                return this.durationField;
            }
            set
            {
                this.durationField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class objectivesObjectiveExpected_progressProgressEntry
    {

        private sbyte checkpointField;

        private byte valueField;

        private byte yearField;

        private bool yearFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public sbyte checkpoint
        {
            get
            {
                return this.checkpointField;
            }
            set
            {
                this.checkpointField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte value
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
        public byte year
        {
            get
            {
                return this.yearField;
            }
            set
            {
                this.yearField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool yearSpecified
        {
            get
            {
                return this.yearFieldSpecified;
            }
            set
            {
                this.yearFieldSpecified = value;
            }
        }
    }

}