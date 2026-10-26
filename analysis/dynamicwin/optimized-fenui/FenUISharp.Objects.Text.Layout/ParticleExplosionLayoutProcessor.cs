using System;
using System.Collections.Generic;
using FenUISharp.Behavior;
using FenUISharp.Mathematics;
using FenUISharp.Objects.Text.Model;
using SkiaSharp;

namespace FenUISharp.Objects.Text.Layout;

public class ParticleExplosionLayoutProcessor : LayoutProcessor
{
	private class Particle
	{
		public SKPoint InitialPosition { get; set; }

		public SKPoint Position { get; set; }

		public SKPoint Velocity { get; set; }

		public float Rotation { get; set; }

		public float RotationVelocity { get; set; }

		public Func<SKColor> Color { get; set; }

		public float Size { get; set; }

		public float Life { get; set; }
	}

	private class ParticleCluster
	{
		public Glyph SourceGlyph { get; set; }

		public List<Particle> Particles { get; set; }
	}

	private List<Glyph>? oldLayout;

	private List<Glyph>? newLayout;

	private List<ParticleCluster>? oldParticles;

	private List<ParticleCluster>? newParticles;

	private AnimatorComponent animatorOut;

	private AnimatorComponent animatorIn;

	private Random random = new Random();

	public bool LowerQualityOnAnimate = true;

	public float Duration { get; init; } = 1.2f;

	public int ParticlesPerGlyph { get; init; } = 12;

	public float BlurMultiplier { get; init; } = 1f;

	public float ExplosionRadius { get; init; } = 80f;

	public float ParticleStartSize { get; init; } = 1f;

	public float ParticleEndSize { get; init; } = 4f;

	public float RotationIntensity { get; init; } = 720f;

	public float GravityStrength { get; init; } = 15f;

	public float VelocityRandomness { get; init; } = 0.6f;

	public float FormationDelay { get; init; } = 0.3f;

	public float PreDelay { get; init; }

	public ParticleExplosionLayoutProcessor(FText parent, TextLayout innerLayout)
		: base(parent, innerLayout)
	{
		parent.OnModelChanged = (Action)Delegate.Combine(parent.OnModelChanged, (Action)delegate
		{
			oldLayout = newLayout;
			if (oldLayout != null)
			{
				oldParticles = new List<ParticleCluster>();
				foreach (Glyph item in oldLayout)
				{
					oldParticles.Add(CreateParticleCluster(item));
				}
			}
			newLayout = null;
			newParticles = null;
			animatorOut?.Restart();
			animatorIn?.Break();
			if (LowerQualityOnAnimate)
			{
				base.Owner.Quality.SetStaticState(0.75f);
			}
		});
		animatorOut = new AnimatorComponent(parent, (float t) => t);
		animatorOut.Duration = Duration;
		AnimatorComponent animatorComponent = animatorOut;
		animatorComponent.OnComplete = (Action)Delegate.Combine(animatorComponent.OnComplete, new Action(OnAnimCompleteOut));
		AnimatorComponent animatorComponent2 = animatorOut;
		animatorComponent2.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent2.OnValueUpdate, new Action<float>(OnAnimValueUpdated));
		animatorIn = new AnimatorComponent(parent, (float t) => t);
		animatorIn.Duration = Duration;
		AnimatorComponent animatorComponent3 = animatorIn;
		animatorComponent3.OnComplete = (Action)Delegate.Combine(animatorComponent3.OnComplete, new Action(OnAnimCompleteIn));
		AnimatorComponent animatorComponent4 = animatorIn;
		animatorComponent4.OnValueUpdate = (Action<float>)Delegate.Combine(animatorComponent4.OnValueUpdate, new Action<float>(OnAnimValueUpdated));
		AnimatorComponent animatorComponent5 = animatorIn;
		animatorComponent5.OnComplete = (Action)Delegate.Combine(animatorComponent5.OnComplete, (Action)delegate
		{
			base.Owner.Quality.SetStaticState(1f);
		});
	}

	private ParticleCluster CreateParticleCluster(Glyph glyph, bool isFormation = false)
	{
		List<Particle> list = new List<Particle>();
		SKPoint position = glyph.Position;
		for (int i = 0; i < ParticlesPerGlyph; i++)
		{
			float num = (float)(random.NextDouble() * Math.PI * 2.0);
			float num2 = (float)(random.NextDouble() * (double)glyph.Size.Width * 0.4000000059604645);
			SKPoint sKPoint = new SKPoint((float)(Math.Cos(num) * (double)num2), (float)(Math.Sin(num) * (double)num2 * 0.6000000238418579));
			SKPoint sKPoint2;
			SKPoint velocity;
			if (isFormation)
			{
				float num3 = ExplosionRadius * (0.7f + (float)random.NextDouble() * 0.6f);
				sKPoint2 = new SKPoint(position.X + (float)(Math.Cos(num) * (double)num3), position.Y + (float)(Math.Sin(num) * (double)num3 * 0.6000000238418579) + GravityStrength * 0.8f);
				velocity = new SKPoint(0f, 0f);
			}
			else
			{
				sKPoint2 = new SKPoint(position.X + sKPoint.X, position.Y + sKPoint.Y);
				velocity = new SKPoint(sKPoint.X * (2f + (float)random.NextDouble() * VelocityRandomness), sKPoint.Y * (1.5f + (float)random.NextDouble() * VelocityRandomness) - GravityStrength * 0.3f);
			}
			list.Add(new Particle
			{
				InitialPosition = sKPoint2,
				Position = sKPoint2,
				Velocity = velocity,
				Rotation = 0f,
				RotationVelocity = (float)((random.NextDouble() - 0.5) * (double)RotationIntensity),
				Color = glyph.Style.Color,
				Size = ParticleStartSize + (float)random.NextDouble() * (ParticleEndSize - ParticleStartSize) * 0.3f,
				Life = 1f
			});
		}
		return new ParticleCluster
		{
			SourceGlyph = glyph,
			Particles = list
		};
	}

	private void OnAnimCompleteOut()
	{
		if (PreDelay >= 0f)
		{
			animatorIn.Restart();
		}
	}

	private void OnAnimCompleteIn()
	{
		oldLayout = null;
		oldParticles = null;
		newParticles = null;
	}

	private void OnAnimValueUpdated(float time)
	{
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
	}

	public override List<Glyph> ProcessModel(TextModel model, SKRect bounds)
	{
		newLayout = base.ProcessModel(model, bounds);
		base.Owner.Invalidate(UIObject.Invalidation.SurfaceDirty);
		List<Glyph> list = new List<Glyph>();
		if (oldParticles != null && animatorOut.IsRunning)
		{
			float time = animatorOut.Time;
			Func<float, float> easing = Easing.EaseInQuart;
			foreach (ParticleCluster oldParticle in oldParticles)
			{
				List<Glyph> collection = UpdateExplosionParticles(oldParticle, time, easing);
				list.AddRange(collection);
			}
		}
		bool flag = false;
		float num = 0f;
		if (PreDelay < 0f && animatorOut.IsRunning)
		{
			float time2 = animatorOut.Time;
			float num2 = 1f + PreDelay;
			if (time2 >= num2)
			{
				flag = true;
				num = (time2 - num2) / (1f - num2);
				if (newParticles == null && newLayout != null)
				{
					newParticles = new List<ParticleCluster>();
					foreach (Glyph item in newLayout)
					{
						newParticles.Add(CreateParticleCluster(item, isFormation: true));
					}
				}
			}
		}
		else if (PreDelay >= 0f && animatorIn.IsRunning)
		{
			flag = true;
			num = animatorIn.Time;
			if (newParticles == null && newLayout != null)
			{
				newParticles = new List<ParticleCluster>();
				foreach (Glyph item2 in newLayout)
				{
					newParticles.Add(CreateParticleCluster(item2, isFormation: true));
				}
			}
		}
		if (newParticles != null && flag)
		{
			float t = Math.Max(0f, (num - FormationDelay) / (1f - FormationDelay));
			Func<float, float> easing2 = Easing.EaseOutQuart;
			foreach (ParticleCluster newParticle in newParticles)
			{
				List<Glyph> collection2 = UpdateFormationParticles(newParticle, t, easing2);
				list.AddRange(collection2);
			}
		}
		if (list.Count > 0)
		{
			return list;
		}
		return newLayout ?? base.ProcessModel(model, bounds);
	}

	private List<Glyph> UpdateExplosionParticles(ParticleCluster cluster, float t, Func<float, float> easing)
	{
		List<Glyph> list = new List<Glyph>();
		float num = easing(t);
		foreach (Particle particle in cluster.Particles)
		{
			particle.Position = new SKPoint(particle.InitialPosition.X + particle.Velocity.X * num * (ExplosionRadius / 50f), particle.InitialPosition.Y + particle.Velocity.Y * num * (ExplosionRadius / 50f) + GravityStrength * num * num);
			particle.Rotation += particle.RotationVelocity * (1f / 60f);
			particle.Life = 1f - num;
			float num2 = RMath.Lerp(ParticleStartSize, ParticleEndSize, num);
			Glyph item = new Glyph('•', particle.Position, new SKSize(num2 / 16f, num2 / 16f), new SKPoint(0.5f, 0.5f), new TextStyle(cluster.SourceGlyph.Style)
			{
				FontSize = num2,
				Opacity = particle.Life * 0.8f,
				BlurRadius = (1f - particle.Life) * 2f * BlurMultiplier
			}, new SKSize(num2, num2));
			list.Add(item);
		}
		return list;
	}

	private List<Glyph> UpdateFormationParticles(ParticleCluster cluster, float t, Func<float, float> easing)
	{
		List<Glyph> list = new List<Glyph>();
		if (t <= 0f)
		{
			return list;
		}
		float num = easing(Math.Min(t, 1f));
		SKPoint sKPoint = new SKPoint(cluster.SourceGlyph.Position.X, cluster.SourceGlyph.Position.Y - cluster.SourceGlyph.Size.Height / 6f);
		foreach (Particle particle in cluster.Particles)
		{
			particle.Position = new SKPoint(RMath.Lerp(particle.InitialPosition.X, sKPoint.X, num), RMath.Lerp(particle.InitialPosition.Y, sKPoint.Y, num));
			particle.Life = num;
			float num2 = RMath.Lerp(ParticleEndSize, ParticleStartSize, num);
			Glyph item = new Glyph('•', particle.Position, new SKSize(num2 / 16f, num2 / 16f), new SKPoint(0.5f, 0.5f), new TextStyle(cluster.SourceGlyph.Style)
			{
				FontSize = num2,
				Opacity = particle.Life * 0.9f,
				BlurRadius = ((1f - particle.Life) * 2f + 1.5f) * BlurMultiplier
			}, new SKSize(num2, num2));
			list.Add(item);
		}
		if (num > 0.7f)
		{
			Glyph item2 = new Glyph(cluster.SourceGlyph.Character, cluster.SourceGlyph.Position, cluster.SourceGlyph.Scale, cluster.SourceGlyph.Anchor, new TextStyle(cluster.SourceGlyph.Style)
			{
				Opacity = RMath.Clamp((num - 0.7f) / 0.3f, 0f, 1f),
				BlurRadius = (1f - num) * 4f * BlurMultiplier
			}, cluster.SourceGlyph.Size);
			list.Add(item2);
		}
		return list;
	}
}
