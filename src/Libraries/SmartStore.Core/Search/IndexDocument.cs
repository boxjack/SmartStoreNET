﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;

namespace SmartStore.Core.Search
{
	public class IndexDocument : IIndexDocument
	{
		private readonly Multimap<string, IndexField> _fields;

		public IndexDocument(int id)
			: this(id, null)
		{
		}

		public IndexDocument(int id, string documentType)
		{
			_fields = new Multimap<string, IndexField>(StringComparer.OrdinalIgnoreCase);
			_fields.Add("id", new IndexField("id", id).Store());

			if (documentType.HasValue())
			{
				_fields.Add("doctype", new IndexField("doctype", documentType).Store());
			}
		}

		public int Id
		{
			get
			{
				return (int)_fields["id"].FirstOrDefault().Value;
			}
		}

		public string DocumentType
		{
			get
			{
				if (_fields.ContainsKey("doctype"))
				{
					return (string)_fields["doctype"].FirstOrDefault().Value;
				}
				return null;
			}
		}

		public int Count => _fields.TotalValueCount;

		public void Add(IndexField field)
		{
			if (field.Name.IsCaseInsensitiveEqual("id") && _fields.ContainsKey("id"))
			{
				// special treatment for id field: allow only one!
				_fields.RemoveAll("id");
			}

			if (field.Name.IsCaseInsensitiveEqual("doctype") && _fields.ContainsKey("doctype"))
			{
				_fields.RemoveAll("doctype");
			}

			_fields.Add(field.Name, field);
		}

		public int Remove(string name)
		{
			if (_fields.ContainsKey(name))
			{
				var num = _fields[name].Count;
				_fields.RemoveAll(name);
				return num;
			}

			return 0;
		}

		public bool Contains(string name)
		{
			return _fields.ContainsKey(name);
		}

		public IEnumerable<IndexField> this[string name]
		{
			get
			{
				if (_fields.ContainsKey(name))
				{
					return _fields[name];
				}

				return Enumerable.Empty<IndexField>();
			}
		}


		public IEnumerator<IndexField> GetEnumerator()
		{
			return _fields.SelectMany(x => x.Value).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}