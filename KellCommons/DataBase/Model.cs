using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace KellCommons.DataBase
{
    public class TypeClass
    {
        public TypeClass()
        {

        }

        public TypeClass(int ID, string TypeName, string Description, int ParentID)
        {
            this.id = ID;
            this.typeName = TypeName;
            this.description = Description;
            this.parentId = ParentID;
        }

        int id;

        public int Id
        {
            get { return id; }
        }
        string typeName = "";

        public string TypeName
        {
            get { return typeName; }
            set { typeName = value; }
        }
        string description = "";

        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        int parentId;

        public int ParentId
        {
            get { return parentId; }
            set { parentId = value; }
        }

        public override string ToString()
        {
            return TypeName;
        }
    }
    public class InfoClass
    {
        public static InfoClass CreateInstance(string Title, int TypeID)
        {
            InfoClass info = new InfoClass();
            info.title = Title;
            info.typeId = TypeID;
            return info;
        }

        public InfoClass()
        {

        }

        public InfoClass(int ID, string Title, int TypeID, bool Visible, DateTime SaveTime)
        {
            this.id = ID;
            this.title = Title;
            this.typeId = TypeID;
            this.visible = Visible;
            this.sysTime = SaveTime;
        }

        int id;

        public int Id
        {
            get { return id; }
        }
        string title = "";

        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        int typeId;

        public int TypeId
        {
            get { return typeId; }
            set { typeId = value; }
        }
        bool visible = true;

        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        DateTime sysTime;

        public DateTime SaveTime
        {
            get { return sysTime; }
            set { sysTime = value; }
        }

        public override string ToString()
        {
            return title;
        }
    }
    public interface IProduct
    {
        int Id
        {
            get;
            set;
        }

        int InfoId
        {
            get;
            set;
        }

        string TypeLink
        {
            get;
            set;
        }

        Dictionary<KeyClass, ValueClass> KeyValues
        {
            get;
        }

        string ToString();
    }
    public class ProductClass : IProduct
    {
        int id;
        int infoId;
            string typeLink;
            Dictionary<KeyClass, ValueClass> keyValues;
            string title;
            List<Image> picture;

            public static ProductClass CreateInstance(string Title, int InfoID, string TypeLink, Dictionary<KeyClass, ValueClass> KeyValues, List<Image> Picture = null)
        {
            ProductClass product = new ProductClass();
            product.title = Title;
            if (Picture != null)
                product.picture = Picture;
            else
                product.picture = new List<Image>();
            product.infoId = InfoID;
            product.typeLink = TypeLink;
            product.keyValues = KeyValues;
            return product;
        }

        public ProductClass()
        {

        }

        public ProductClass(int ID, int InfoID, string Title, string TypeLink, Dictionary<KeyClass, ValueClass> KeyValues, List<Image> Picture = null)
        {
            this.id = ID;
            this.infoId = InfoID;
            this.typeLink = TypeLink;
            this.keyValues = KeyValues;
            this.title = Title;
            if (Picture != null)
                this.picture = Picture;
            else
                this.picture = new List<Image>();
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public List<Image> Picture
        {
            get { return picture; }
            set { picture = value; }
        }

        public override string ToString()
        {
            return TypeLink + ": " + title;
        }

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public int InfoId
        {
            get
            {
                return infoId;
            }
            set
            {
                infoId = value;
            }
        }

        public string TypeLink
        {
            get
            {
                return typeLink;
            }
            set
            {
                typeLink = value;
            }
        }

        public Dictionary<KeyClass, ValueClass> KeyValues
        {
            get
            {
                return keyValues;
            }
            //set
            //{
            //    keyvalues = value;
            //}
        }
    }
    public class KeyClass
    {
        int id;

        public int Id
        {
            get { return id; }
        }
        int typeId;

        public int TypeId
        {
            get { return typeId; }
            set { typeId = value; }
        }
        string keyName;

        public string KeyName
        {
            get { return keyName; }
            set { keyName = value; }
        }
        string friendName;

        public string FriendName
        {
            get { return friendName; }
            set { friendName = value; }
        }
        string unit;

        public string Unit
        {
            get { return unit; }
            set { unit = value; }
        }

        public KeyClass()
        {

        }

        public KeyClass(int ID, int TypeId, string KeyName, string FriendName, string Unit)
        {
            id = ID;
            typeId = TypeId;
            keyName = KeyName;
            friendName = FriendName;
            unit = Unit;
        }

        public override string ToString()
        {
            return keyName + ":" + friendName;
        }
    }
    public class ValueClass
    {
        int id;

        public int Id
        {
            get { return id; }
        }
        int infoId;

        public int InfoId
        {
            get { return infoId; }
            set { infoId = value; }
        }
        int keyId;

        public int KeyId
        {
            get { return keyId; }
            set { keyId = value; }
        }
        string valueString;

        public string ValueString
        {
            get { return valueString; }
            set { valueString = value; }
        }

        public ValueClass()
        {

        }

        public ValueClass(int ID, int InfoId, int KeyId, string ValueString)
        {
            id = ID;
            infoId = InfoId;
            keyId = KeyId;
            valueString = ValueString;
        }
    }
    public class TypeNode : KellControls.KellNode
    {
        TypeClass type;
        List<TypeClass> types;

        public TypeNode(TypeClass type)
            : base(type.Id.ToString())
        {
            this.type = type;
        }

        public TypeNode(List<TypeClass> types)
            : base()
        {
            this.type = null;
            this.types = types;
        }

        public override List<KellControls.KellNode> GetSub()
        {
            List<KellControls.KellNode> children = new List<KellControls.KellNode>();
            List<TypeClass> ts = null;
            if (type != null)
                ts = OleBLL.GetSubTypes(type.Id);
            else
                ts = types;
            foreach (TypeClass t in ts)
            {
                children.Add(new TypeNode(t));
            }
            return children;
        }
    }
}
