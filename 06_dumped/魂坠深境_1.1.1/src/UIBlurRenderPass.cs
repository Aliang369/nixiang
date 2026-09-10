using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIBlurRenderPass : ScriptableRenderPass
{
	public int downsample;

	public float strength;

	private Material material;

	private RTHandle blurTex0;

	private RTHandle blurTex1;

	public UIBlurRenderPass()
	{
		((ScriptableRenderPass)this).ConfigureInput((ScriptableRenderPassInput)4);
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		if (material == null)
		{
			Shader shader = Shader.Find("Hidden/Universal Render Pipeline/UIBlurBackground");
			material = CoreUtils.CreateEngineMaterial(shader);
		}
	}

	internal void ReAllocate(ref RTHandle rt, RenderTextureDescriptor desc, string name)
	{
		desc.msaaSamples = 1;
		desc.depthBufferBits = 0;
		RenderingUtils.ReAllocateIfNeeded(ref rt, ref desc, FilterMode.Point, TextureWrapMode.Repeat, false, 1, 0f, name);
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.width /= downsample;
		cameraTargetDescriptor.height /= downsample;
		ReAllocate(ref blurTex0, cameraTargetDescriptor, "_BlurTex0");
		ReAllocate(ref blurTex1, cameraTargetDescriptor, "_BlurTex1");
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get("UI Blur Background");
		int num = Mathf.CeilToInt(strength * 6f);
		if (num % 2 == 0)
		{
			num++;
		}
		material.SetInteger("_GridSize", num);
		material.SetFloat("_Spread", strength);
		RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
		RTHandle cameraColorTargetHandle2 = renderingData.cameraData.renderer.cameraColorTargetHandle;
		Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, blurTex0, material, 0);
		Blitter.BlitCameraTexture(commandBuffer, blurTex0, blurTex1, material, 1);
		Blitter.BlitCameraTexture(commandBuffer, blurTex1, cameraColorTargetHandle2);
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
		CommandBufferPool.Release(commandBuffer);
	}

	public void ReleaseTargets()
	{
		RTHandles.Release(blurTex0);
		RTHandles.Release(blurTex1);
	}
}
