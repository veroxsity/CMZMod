using System;
using System.Collections.Generic;
using DNA.Collections;
using DNA.Drawing.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class Entity : Tree<Entity>
	{
		public const int DefaultDrawPriority = 0;

		public Color? EntityColor;

		private Vector3 _localPosition = Vector3.Zero;

		private Quaternion _localRotation = Quaternion.Identity;

		private Vector3 _localScale = new Vector3(1f, 1f, 1f);

		private Matrix _localToWorld;

		private bool _ltwDirty = true;

		private State _currentAction;

		private Queue<State> _actionQueue = new Queue<State>();

		public int DrawPriority;

		public static SamplerState DefaultSamplerState = SamplerState.AnisotropicWrap;

		public static BlendState DefaultBlendState = BlendState.Opaque;

		public static RasterizerState DefaultRasterizerState = RasterizerState.CullCounterClockwise;

		public static DepthStencilState DefaultDepthStencilState = DepthStencilState.Default;

		private SamplerState _samplerState;

		private BlendState _blendState;

		private RasterizerState _rasterizerState;

		private DepthStencilState _depthStencilState;

		private Physics _physics;

		public bool AlphaSort;

		public bool Visible = true;

		public bool DoUpdate = true;

		public bool Collider;

		public bool Collidee;

		private string _name;

		private static Plane EmptyPlane = default(Plane);

		private bool _ltpDirty = true;

		private Matrix _cachedLocalToParent;

		public SamplerState SamplerState
		{
			get
			{
				if (_samplerState != null)
				{
					return _samplerState;
				}
				Entity parent = base.Parent;
				if (parent == null)
				{
					return DefaultSamplerState;
				}
				return parent.SamplerState;
			}
			set
			{
				_samplerState = value;
			}
		}

		public BlendState BlendState
		{
			get
			{
				if (_blendState != null)
				{
					return _blendState;
				}
				Entity parent = base.Parent;
				if (parent == null)
				{
					return DefaultBlendState;
				}
				return parent.BlendState;
			}
			set
			{
				_blendState = value;
			}
		}

		public RasterizerState RasterizerState
		{
			get
			{
				if (_rasterizerState != null)
				{
					return _rasterizerState;
				}
				Entity parent = base.Parent;
				if (parent == null)
				{
					return DefaultRasterizerState;
				}
				return parent.RasterizerState;
			}
			set
			{
				_rasterizerState = value;
			}
		}

		public DepthStencilState DepthStencilState
		{
			get
			{
				if (_depthStencilState != null)
				{
					return _depthStencilState;
				}
				Entity parent = base.Parent;
				if (parent == null)
				{
					return DefaultDepthStencilState;
				}
				return parent.DepthStencilState;
			}
			set
			{
				_depthStencilState = value;
			}
		}

		public Physics Physics
		{
			get
			{
				return _physics;
			}
			set
			{
				_physics = value;
				if (_physics != null && _physics.Owner != this)
				{
					throw new Exception();
				}
			}
		}

		public Queue<State> ActionQueue
		{
			get
			{
				return _actionQueue;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public Matrix LocalToParent
		{
			get
			{
				if (_ltpDirty)
				{
					_cachedLocalToParent = Matrix.CreateScale(_localScale) * Matrix.CreateFromQuaternion(_localRotation);
					_cachedLocalToParent.Translation = _localPosition;
				}
				return _cachedLocalToParent;
			}
			set
			{
				_cachedLocalToParent = value;
				_ltpDirty = false;
				value.Decompose(out _localScale, out _localRotation, out _localPosition);
				DirtyLTW();
			}
		}

		protected bool LTWDirty
		{
			get
			{
				return _ltwDirty;
			}
		}

		public Matrix LocalToWorld
		{
			get
			{
				if (_ltwDirty)
				{
					if (base.Parent == null)
					{
						_localToWorld = LocalToParent;
					}
					else
					{
						_localToWorld = LocalToParent * base.Parent.LocalToWorld;
					}
					_ltwDirty = false;
				}
				return _localToWorld;
			}
		}

		public Matrix WorldToLocal
		{
			get
			{
				return Matrix.Invert(LocalToWorld);
			}
		}

		public Vector3 WorldPosition
		{
			get
			{
				if (_ltwDirty)
				{
					return LocalToWorld.Translation;
				}
				return _localToWorld.Translation;
			}
		}

		public Vector3 LocalPosition
		{
			get
			{
				return _localPosition;
			}
			set
			{
				_localPosition = value;
				_ltpDirty = true;
				DirtyLTW();
			}
		}

		public Vector3 LocalScale
		{
			get
			{
				return _localScale;
			}
			set
			{
				_localScale = value;
				_ltpDirty = true;
				DirtyLTW();
			}
		}

		public Quaternion LocalRotation
		{
			get
			{
				return _localRotation;
			}
			set
			{
				_localRotation = value;
				_localRotation.Normalize();
				_ltpDirty = true;
				DirtyLTW();
			}
		}

		public Scene Scene
		{
			get
			{
				Entity entity = this;
				while (true)
				{
					if (entity == null)
					{
						return null;
					}
					if (entity is Scene || entity == null)
					{
						break;
					}
					entity = entity.Parent;
				}
				return (Scene)entity;
			}
		}

		public State CurrentAction
		{
			get
			{
				return _currentAction;
			}
		}

		public void ApplyEffect(Effect effect, bool applyToChildren)
		{
			OnApplyEffect(effect);
			if (!applyToChildren)
			{
				return;
			}
			foreach (Entity child in base.Children)
			{
				child.ApplyEffect(effect, applyToChildren);
			}
		}

		public virtual BoundingSphere GetLocalBoundingSphere()
		{
			return new BoundingSphere(new Vector3(0f, 0f, 0f), 0f);
		}

		public virtual BoundingBox GetAABB()
		{
			return new BoundingBox(WorldPosition, WorldPosition);
		}

		protected virtual void OnApplyEffect(Effect sourceEffect)
		{
		}

		public void SetRenderState(GraphicsDevice device)
		{
			Entity parent = base.Parent;
			if (parent == null)
			{
				if (_samplerState != null)
				{
					device.SamplerStates[0] = _samplerState;
				}
				else
				{
					device.SamplerStates[0] = DefaultSamplerState;
				}
				if (_blendState != null)
				{
					device.BlendState = _blendState;
				}
				else
				{
					device.BlendState = DefaultBlendState;
				}
				if (_rasterizerState != null)
				{
					device.RasterizerState = _rasterizerState;
				}
				else
				{
					device.RasterizerState = DefaultRasterizerState;
				}
				if (_depthStencilState != null)
				{
					device.DepthStencilState = _depthStencilState;
				}
				else
				{
					device.DepthStencilState = DefaultDepthStencilState;
				}
			}
			else
			{
				if (_samplerState != null)
				{
					device.SamplerStates[0] = _samplerState;
				}
				else
				{
					device.SamplerStates[0] = parent.SamplerState;
				}
				if (_blendState != null)
				{
					device.BlendState = _blendState;
				}
				else
				{
					device.BlendState = parent.BlendState;
				}
				if (_rasterizerState != null)
				{
					device.RasterizerState = _rasterizerState;
				}
				else
				{
					device.RasterizerState = parent.RasterizerState;
				}
				if (_depthStencilState != null)
				{
					device.DepthStencilState = _depthStencilState;
				}
				else
				{
					device.DepthStencilState = parent.DepthStencilState;
				}
			}
		}

		protected void GetDrawList(List<Light> lights, List<Entity> toSort, List<Entity> toDraw, FilterCallback<Entity> filter)
		{
			for (int i = 0; i < base.Children.Count; i++)
			{
				Entity entity = base.Children[i];
				if (!entity.Visible || !filter(entity))
				{
					continue;
				}
				if (entity is Light)
				{
					lights.Add((Light)entity);
				}
				else if (AlphaSort)
				{
					if (AlphaSort)
					{
						toSort.Add(entity);
					}
					else
					{
						toDraw.Add(entity);
					}
					entity.GetDrawList(lights, toSort, toDraw, filter);
				}
				else
				{
					toDraw.Add(entity);
				}
				entity.GetDrawList(lights, toSort, toDraw, filter);
			}
		}

		public virtual void ResolveCollsions(List<Entity> collidees, GameTime dt)
		{
			for (int i = 0; i < collidees.Count; i++)
			{
				Entity e = collidees[i];
				Plane collsionPlane;
				if (CollidesAgainist(e) && ResolveCollsion(e, out collsionPlane, dt))
				{
					OnCollisionWith(e, collsionPlane);
				}
			}
		}

		public virtual void OnCollisionWith(Entity e, Plane collsionPlane)
		{
		}

		public virtual bool CollidesAgainist(Entity e)
		{
			if (e != this && Collider)
			{
				return e.Collidee;
			}
			return false;
		}

		public virtual bool ResolveCollsion(Entity e, out Plane collsionPlane, GameTime dt)
		{
			collsionPlane = EmptyPlane;
			return false;
		}

		public void RemoveFromParent()
		{
			if (base.Parent != null)
			{
				base.Parent.Children.Remove(this);
			}
		}

		protected virtual void OnMoved()
		{
		}

		private void DirtyLTW()
		{
			OnMoved();
			_ltwDirty = true;
			for (int i = 0; i < base.Children.Count; i++)
			{
				Entity entity = base.Children[i];
				if (!entity._ltwDirty)
				{
					entity.DirtyLTW();
				}
			}
		}

		public Entity()
			: base(20)
		{
		}

		protected override void OnParentChanged(Entity oldParent, Entity newParent)
		{
			DirtyLTW();
			base.OnParentChanged(oldParent, newParent);
		}

		public void AdoptChild(Entity child)
		{
			Vector3 worldPosition = WorldPosition;
			Matrix localToWorld = child.LocalToWorld;
			Matrix worldToLocal = WorldToLocal;
			Matrix matrix = localToWorld * worldToLocal;
			if (child.Parent != null)
			{
				child.RemoveFromParent();
			}
			base.Children.Add(child);
			Vector3 scale;
			Quaternion rotation;
			Vector3 translation;
			matrix.Decompose(out scale, out rotation, out translation);
			child.LocalPosition = translation;
			child.LocalRotation = rotation;
			child.LocalScale = scale;
			Vector3 worldPosition2 = WorldPosition;
		}

		protected virtual void OnActionStarted(State action)
		{
		}

		protected virtual void OnActionComplete(State action)
		{
		}

		public void InjectAction(State action)
		{
			InjectActions(new State[1] { action });
		}

		public void InjectActions(IList<State> actions)
		{
			Queue<State> queue = new Queue<State>();
			for (int i = 0; i < actions.Count; i++)
			{
				queue.Enqueue(actions[i]);
			}
			if (_currentAction != null)
			{
				queue.Enqueue(_currentAction);
				_currentAction = null;
			}
			foreach (State item in _actionQueue)
			{
				queue.Enqueue(item);
			}
			_actionQueue = queue;
		}

		private void NextAction()
		{
			if (_currentAction == null && _actionQueue.Count > 0)
			{
				_currentAction = _actionQueue.Dequeue();
				_currentAction.Start(this);
				OnActionStarted(_currentAction);
			}
		}

		public void EndCurrentAction()
		{
			if (_currentAction != null)
			{
				_currentAction.End(this);
				_currentAction = null;
				OnActionComplete(_currentAction);
			}
		}

		public void ResetActions()
		{
			EndCurrentAction();
			_actionQueue.Clear();
		}

		protected virtual void OnUpdate(GameTime gameTime)
		{
		}

		public virtual void OnPhysics(GameTime gameTime)
		{
		}

		public virtual void Update(DNAGame game, GameTime gameTime)
		{
			if (!DoUpdate)
			{
				return;
			}
			NextAction();
			if (_currentAction != null)
			{
				_currentAction.Tick(game, this, gameTime);
				if (_currentAction != null && _currentAction.Complete)
				{
					EndCurrentAction();
				}
			}
			NextAction();
			OnUpdate(gameTime);
			for (int i = 0; i < base.Children.Count; i++)
			{
				base.Children[i].Update(game, gameTime);
			}
		}

		public virtual void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
		}

		public void AfterFrame()
		{
			OnAfterFrame();
			for (int i = 0; i < base.Children.Count; i++)
			{
				base.Children[i].AfterFrame();
			}
		}

		public virtual void OnAfterFrame()
		{
		}
	}
}
