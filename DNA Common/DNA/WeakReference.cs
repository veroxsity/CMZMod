using System;

namespace DNA
{
	public class WeakReference<T> : WeakReference
	{
		public new T Target
		{
			get
			{
				return (T)base.Target;
			}
			set
			{
				base.Target = value;
			}
		}

		public WeakReference(object o)
			: base(o)
		{
		}

		public WeakReference(object o, bool trackResurection)
			: base(o, trackResurection)
		{
		}
	}
}
