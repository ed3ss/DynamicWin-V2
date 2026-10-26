using System;
using SkiaSharp;
using Vortice.Direct3D12;

namespace FenUISharp;

public class FAdditionalSurface : IDisposable
{
	public SKSurface SkiaSurface { get; private set; }

	public ID3D12Resource Texture { get; private set; }

	public GRD3DTextureResourceInfo ResourceInfo { get; private set; }

	public GRBackendTexture BackendTexture { get; private set; }

	public SKImageInfo Info { get; private set; }

	private SkiaDirectCompositionContext SkiaDirectCompositionContext { get; set; }

	public FAdditionalSurface(SKSurface s, ID3D12Resource d, GRD3DTextureResourceInfo r, GRBackendTexture t, SKImageInfo info, SkiaDirectCompositionContext skiaDirectCompositionContext)
	{
		SkiaSurface = s;
		Texture = d;
		ResourceInfo = r;
		BackendTexture = t;
		Info = info;
		SkiaDirectCompositionContext = skiaDirectCompositionContext;
		SkiaDirectCompositionContext skiaDirectCompositionContext2 = SkiaDirectCompositionContext;
		skiaDirectCompositionContext2.OnRebuildAdditionals = (Action)Delegate.Combine(skiaDirectCompositionContext2.OnRebuildAdditionals, new Action(RebuildSurface));
		SkiaDirectCompositionContext skiaDirectCompositionContext3 = SkiaDirectCompositionContext;
		skiaDirectCompositionContext3.OnDisposeAdditionals = (Action)Delegate.Combine(skiaDirectCompositionContext3.OnDisposeAdditionals, new Action(Dispose));
	}

	private void RebuildSurface()
	{
		Dispose();
		SkiaDirectCompositionContext skiaDirectCompositionContext = SkiaDirectCompositionContext;
		skiaDirectCompositionContext.OnRebuildAdditionals = (Action)Delegate.Combine(skiaDirectCompositionContext.OnRebuildAdditionals, new Action(RebuildSurface));
		SkiaDirectCompositionContext skiaDirectCompositionContext2 = SkiaDirectCompositionContext;
		skiaDirectCompositionContext2.OnDisposeAdditionals = (Action)Delegate.Combine(skiaDirectCompositionContext2.OnDisposeAdditionals, new Action(Dispose));
		FAdditionalSurface fAdditionalSurface = SkiaDirectCompositionContext.CreateAdditional(Info);
		SkiaSurface = fAdditionalSurface.SkiaSurface;
		Texture = fAdditionalSurface.Texture;
		ResourceInfo = fAdditionalSurface.ResourceInfo;
		BackendTexture = fAdditionalSurface.BackendTexture;
	}

	public void Dispose()
	{
		SkiaSurface?.Dispose();
		SkiaSurface = null;
		Texture?.Dispose();
		Texture = null;
		ResourceInfo?.Dispose();
		ResourceInfo = null;
		BackendTexture?.Dispose();
		BackendTexture = null;
		SkiaDirectCompositionContext skiaDirectCompositionContext = SkiaDirectCompositionContext;
		skiaDirectCompositionContext.OnRebuildAdditionals = (Action)Delegate.Remove(skiaDirectCompositionContext.OnRebuildAdditionals, new Action(RebuildSurface));
		SkiaDirectCompositionContext skiaDirectCompositionContext2 = SkiaDirectCompositionContext;
		skiaDirectCompositionContext2.OnDisposeAdditionals = (Action)Delegate.Remove(skiaDirectCompositionContext2.OnDisposeAdditionals, new Action(Dispose));
	}
}
