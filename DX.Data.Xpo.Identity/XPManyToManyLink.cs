using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DX.Data.Xpo.Identity
{
	/// <summary>
	/// The links of a many-to-many association (user - role), worked on through XPO's intermediate objects.
	/// Adding or removing a link through the collection on one side makes XPO load the complete collection of the other
	/// side (every user of the role) to keep it in sync; the intermediate objects touch only the links concerned.
	/// The persistent schema is unchanged: these are the rows of the intermediate table XPO already has.
	/// </summary>
	internal sealed class XPManyToManyLink
	{
		private XPManyToManyLink(XPClassInfo intermediate, XPMemberInfo ownerReference, XPMemberInfo otherReference)
		{
			Intermediate = intermediate;
			OwnerReference = ownerReference;
			OtherReference = otherReference;
		}

		public XPClassInfo Intermediate { get; }
		/// <summary>The member of the intermediate class that refers to the owner (the user)</summary>
		public XPMemberInfo OwnerReference { get; }
		/// <summary>The member of the intermediate class that refers to the other side (the role)</summary>
		public XPMemberInfo OtherReference { get; }

		/// <summary>
		/// The many-to-many association between <paramref name="ownerType"/> and <paramref name="otherType"/>, or null if there
		/// is none XPO keeps in an intermediate table (or it cannot be told apart): then use the collections
		/// </summary>
		public static XPManyToManyLink? Find(Session session, Type ownerType, Type otherType)
		{
			var owner = session.GetClassInfo(ownerType);
			var other = session.GetClassInfo(otherType);
			XPManyToManyLink? found = null;
			foreach (XPMemberInfo member in owner.CollectionProperties)
			{
				if (!member.IsManyToMany || member.IntermediateClass == null || member.CollectionElementType == null)
					continue;
				if (!other.IsAssignableTo(member.CollectionElementType))
					continue;
				var references = member.IntermediateClass.ObjectProperties.Cast<XPMemberInfo>().Where(m => m.ReferenceType != null).ToList();
				var ownerReferences = references.Where(m => owner.IsAssignableTo(m.ReferenceType)).ToList();
				var otherReferences = references.Where(m => other.IsAssignableTo(m.ReferenceType)).ToList();
				if (found != null || ownerReferences.Count != 1 || otherReferences.Count != 1 || ownerReferences[0] == otherReferences[0])
					return null; // ambiguous
				found = new XPManyToManyLink(member.IntermediateClass, ownerReferences[0], otherReferences[0]);
			}
			return found;
		}

		/// <summary>Adds the link (in the session; committed with it)</summary>
		public void Add(Session session, object owner, object other)
		{
			var link = Intermediate.CreateNewObject(session);
			OwnerReference.SetValue(link, owner);
			OtherReference.SetValue(link, other);
			session.Save(link);
		}

		public CriteriaOperator OwnerIs(object ownerKey) => new BinaryOperator(OwnerReference.Name, ownerKey);

		/// <summary>The links that match <paramref name="criteria"/> (on the intermediate class: OwnerReference / OtherReference)</summary>
		public XPCollection Links(Session session, CriteriaOperator criteria) => new XPCollection(session, Intermediate, criteria);
	}
}
