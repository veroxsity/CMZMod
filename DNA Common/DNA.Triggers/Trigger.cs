namespace DNA.Triggers
{
	public abstract class Trigger
	{
		private bool _oneShot;

		private bool _triggered;

		private bool _lastState;

		public bool Triggered
		{
			get
			{
				return _triggered;
			}
		}

		public Trigger(bool oneShot)
		{
			_oneShot = oneShot;
		}

		protected abstract bool IsSastisfied();

		public virtual void OnTriggered()
		{
		}

		public virtual void Reset()
		{
			_triggered = false;
		}

		public void Update()
		{
			if (!_oneShot || !_triggered)
			{
				OnUpdate();
				bool flag = IsSastisfied();
				if (!_lastState && flag)
				{
					_triggered = true;
					OnTriggered();
				}
				_lastState = flag;
			}
		}

		protected virtual void OnUpdate()
		{
		}
	}
}
