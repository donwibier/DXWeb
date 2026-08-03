using DX.Utils.MimeDetection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace DX.Utils.MimeDetection
{
	/// <summary>
    /// Defines a Mime Content Type
	/// </summary>
	public sealed class MimeType
	{
		#region Private Members
		/// <summary>The primary and sub types separator </summary>
		private const string SEPARATOR = "/";
		
		/// <summary>The parameters separator </summary>
		private const string PARAMS_SEP = ";";
		
		/// <summary>Special characters not allowed in content types. </summary>
		private const string SPECIALS = "()<>@,;:\\\"/[]?=";

		/// <summary>The Mime-Type associated extensions </summary>
		private List<string> _Extensions = new List<string>();

		/// <summary>The magic bytes associated to this Mime-Type </summary>
		private List<Magic> MagicBytes = new List<Magic>();
		
		#endregion

		#region Class Constructor
		/// <summary> Creates a MimeType from a String.</summary>
		/// <param name="name">the MIME content type String.
		/// </param>
		public MimeType(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

            // Split the two parts of the Mime Content Type
            string[] parts = name.Split(new char[] { SEPARATOR[0] }, 2);
			// Checks validity of the parts
			if (parts.Length != 2)
				throw new MimeTypeException("Invalid Content Type " + name);
			
			Name = $"{parts[0]}{SEPARATOR}{PrepType(parts[1])}";
			PrimaryType = parts[0];
			SubType = PrepType(parts[1]);
		}

		/// <summary> Creates a MimeType with the given primary type and sub type.</summary>
		/// <param name="primary">the content type primary type.
		/// </param>
		/// <param name="sub">the content type sub type.
		/// </param>
		public MimeType(string primary, string sub)
		{
            if (string.IsNullOrEmpty(primary))
                throw new ArgumentNullException(nameof(primary));

			// Preliminary checks...
			if (string.IsNullOrEmpty(primary) || (!IsValid(primary)))
				throw new MimeTypeException("Invalid Primary Type " + primary);

			// All is ok, assign values
			Name = $"{primary}{SEPARATOR}{PrepType(sub)}";
			PrimaryType = primary;
			SubType = PrepType(sub);
		}

		private string PrepType(string type)
		{
			// Remove optional parameters from the sub type
			string result = default!;
			if (!string.IsNullOrEmpty(type))
				result = type.Split(PARAMS_SEP[0])[0];

			if (string.IsNullOrEmpty(result) || (!IsValid(result)))			
				throw new MimeTypeException("Invalid Sub Type " + result);
			
			return result;
		}

		/// <summary>Checks if the specified primary or sub type is valid. </summary>
		private bool IsValid(string type) => !string.IsNullOrEmpty(type) && !HasCtrlOrSpecials(type);

		/// <summary>Checks if the specified string contains some special characters. </summary>
		private bool HasCtrlOrSpecials(string type)
		{
			int len = type.Length, i = 0;			
			while (i < len)
			{
				char c = type[i];
				if (c <= '\x001A' || SPECIALS.IndexOf(c) > 0)
					return true;
				i++;
			}
			return false;
		}
#endregion

		#region Public Properties
		/// <summary> Return the name of this mime-type.</summary>
		/// <returns> the name of this mime-type.
		/// </returns>
		public string Name { get; }
		/// <summary> Return the primary type of this mime-type.</summary>
		/// <returns> the primary type of this mime-type.
		/// </returns>
		public string PrimaryType { get; } = null!;
		/// <summary> Return the sub type of this mime-type.</summary>
		/// <returns> the sub type of this mime-type.
		/// </returns>
		public string SubType { get; }

		/// <summary> Return the description of this mime-type.</summary>
		/// <returns> the description of this mime-type.
		/// </returns>
		/// <summary> Set the description of this mime-type.</summary>
		/// <param name="description">the description of this mime-type.
		/// </param>
		internal string Description { get; set; } = null!;
		/// <summary> Return the extensions of this mime-type</summary>
		/// <returns> the extensions associated to this mime-type.
		/// </returns>
		internal string[] Extensions => _Extensions.ToArray();
			//(string[])SupportUtil.ToArray(_Extensions, new string[_Extensions.Count]);
		internal int MinLength { get; private set;  } = 0;
		#endregion

		#region Public Methods
		/// <summary> Cleans a content-type.
		/// This method cleans a content-type by removing its optional parameters
		/// and returning only its <code>primary-type/sub-type</code>.
		/// </summary>
		/// <param name="type">is the content-type to clean.
		/// </param>
		/// <returns> the cleaned version of the specified content-type.
		/// </returns>
		/// <throws>  MimeTypeException if something wrong occurs during the </throws>
		/// <summary>         parsing/cleaning of the specified type.
		/// </summary>
		public static string Clean(string type) => (new MimeType(type)).Name;

		public override string ToString() => Name;

		/// <summary> Indicates if an object is equal to this mime-type.
		/// The specified object is equal to this mime-type if it is not null, and
		/// it is an instance of MimeType and its name is equals to this mime-type.
		/// 
		/// </summary>
		/// <param name="obj">the reference object with which to compare.
		/// </param>
		/// <returns> <code>true</code> if this mime-type is equal to the object
		/// argument; <code>false</code> otherwise.
		/// </returns>

		public override bool Equals(object obj)
		{
			var m = obj as MimeType;
			if (m == null)
				return false;

			return m.Name.Equals(Name);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode() => Name.GetHashCode();

		public int MagicLength => MagicBytes.Count();

		public bool Matches(string url)
		{
			bool match = false;
			int index = url.LastIndexOf('.');
			if ((index != -1) && (index < url.Length - 1))
			{
				// There's an extension, so try to find if it matches mines
				match = _Extensions.Contains(url.Substring(index + 1));
			}
			return match;
		}


		#endregion


		/// <summary> Add a supported extension.</summary>
		/// <param name="the">extension to add to the list of extensions associated
		/// to this mime-type.
		/// </param>
		internal void AddExtension(string ext)
		{
			_Extensions.Add(ext);
		}
		
		internal void AddMagic(int offset, string type, string magic)
		{
			// Some preliminary checks...
			if (string.IsNullOrEmpty(magic))
			{
				return ;
			}
            Magic m = new Magic(this, offset, type, magic);
			if (m != null)
			{
				MagicBytes.Add(m);
				MinLength = Math.Max(MinLength, m.Size());
			}
		}
		
		internal bool HasMagic()
		{
			return (MagicBytes.Count > 0);
		}

		internal bool Matches(sbyte[] data)
		{
			if (!HasMagic())
			{
				return false;
			}

            Magic tested = null!;
			for (int i = 0; i < MagicBytes.Count; i++)
			{
				tested = (Magic) MagicBytes[i]!;
				if (tested != null && tested.Matches(data))
				{
					return true;
				}
			}
			return false;
		}

		#region Magic Class
 		private class Magic
		{
			private int _Offset;
			private sbyte[] _MagicBytes = { };
            private MimeType _MimeType;
			
			internal Magic(MimeType obMimeType, int offset, string type, string magic)
			{
				if (obMimeType == null)
				throw new ArgumentNullException(nameof(obMimeType));

                _MimeType = obMimeType;
				this._Offset = offset;
				
				if ((type != null) && (type.Equals("System.Byte")))
					this._MagicBytes = ReadBytes(magic);
				else
                    this._MagicBytes = SupportUtil.ToSByteArray(SupportUtil.ToByteArray(magic));
			}
			
			internal int Size() => (_Offset + _MagicBytes.Length);
			//internal bool Matches(sbyte[] data) => _MagicBytes.SequenceEqual(data.Skip(_Offset));

			internal bool Matches(sbyte[] data)
			{
				if (data == null)
					throw new ArgumentNullException(nameof(data));

				int idx = _Offset;
				if ((idx + _MagicBytes.Length) > data.Length)
					return false;

				for (int i = 0; i < _MagicBytes.Length; i++)
				{
					if (_MagicBytes[i] != data[idx++])
						return false;
				}
				return true;
			}

			//private sbyte[] ReadBytes(string magic)
			//{
			//	if (magic.Length % 2 != 0) return null!;
			//	var data = new sbyte[magic.Length / 2];
			//	for (int i = 0; i < magic.Length; i += 2)
			//	{
			//		if (!byte.TryParse(magic.Substring(i, 2), System.Globalization.NumberStyles.HexNumber, null, out byte b))
			//			throw new ArgumentException();
			//		data[i / 2] = (sbyte)b;
			//	}
			//	return data;
			//}

			private sbyte[] ReadBytes(string magic)
			{
				sbyte[] data = null!;

				if ((magic.Length % 2) == 0)
				{
					string tmp = magic.ToLower();
					data = new sbyte[tmp.Length / 2];
					int byteValue = 0;
					for (int i = 0; i < tmp.Length; i++)
					{
						char c = tmp[i];
						int number;
						if (c >= '0' && c <= '9')
						{
							number = c - '0';
						}
						else if (c >= 'a' && c <= 'f')
						{
							number = 10 + c - 'a';
						}
						else
						{
							throw new System.ArgumentException();
						}
						if ((i % 2) == 0)
						{
							byteValue = number * 16;
						}
						else
						{
							byteValue += number;
							data[i / 2] = (sbyte)byteValue;
						}
					}
				}
				return data;
			}

			/// <summary>
			/// 
			/// </summary>
			/// <returns></returns>
			public override string ToString() => $"[{_Offset}/{SupportUtil.ToByteArray(_MagicBytes)}]";
		}
		#endregion
	}
}