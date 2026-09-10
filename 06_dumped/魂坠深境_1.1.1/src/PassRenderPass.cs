using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PassRenderPass : ScriptableRenderPass
{
	private FilteringSettings m_FilteringSettings;

	private RenderStateBlock m_RenderStateBlock;

	private readonly List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

	private readonly PassRenderFeature.RenderPassSettings settings;

	public PassRenderPass(PassRenderFeature.RenderPassSettings settings)
	{
		this.settings = settings;
		SetupPassInfo();
		SetupFilter();
		SetStencilState();
	}

	private void SetupPassInfo()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		((ScriptableRenderPass)this).renderPassEvent = settings.renderPassEvent;
	}

	private void SetupFilter()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		RenderQueueRange value = (((int)settings.filterSettings.renderQueueType == 1) ? RenderQueueRange.transparent : RenderQueueRange.opaque);
		m_FilteringSettings = new FilteringSettings(value, settings.filterSettings.layerMask);
		string[] passNames = settings.filterSettings.passNames;
		if (passNames != null && passNames.Length != 0)
		{
			string[] array = passNames;
			foreach (string name in array)
			{
				m_ShaderTagIdList.Add(new ShaderTagId(name));
			}
		}
		else
		{
			m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
			m_ShaderTagIdList.Add(new ShaderTagId("LightweightForward"));
			m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
			m_ShaderTagIdList.Add(new ShaderTagId("DepthOnly"));
		}
	}

	private void SetStencilState()
	{
		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		SortingCriteria sortingCriteria = (((int)settings.filterSettings.renderQueueType == 1) ? SortingCriteria.CommonTransparent : renderingData.cameraData.defaultOpaqueSortFlags);
		DrawingSettings drawingSettings = ((ScriptableRenderPass)this).CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
		CommandBuffer commandBuffer = CommandBufferPool.Get(settings.passTag);
		if (settings.cleanRenderTextureColor || settings.cleanRenderTextureDepth)
		{
			commandBuffer.ClearRenderTarget(settings.cleanRenderTextureDepth, settings.cleanRenderTextureColor, Color.black);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
		}
		context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
