// https://wiki.unity3d.com/index.php/VertexColorUnlit#ShaderLab_-_VertexColorUnlit.shader

Shader "Vertex Color Unlit" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	}

		Category{
			Tags { "Queue" = "Geometry" }
			Lighting Off
			BindChannels {
				Bind "Color", color
				Bind "Vertex", vertex
				Bind "TexCoord", texcoord
			}

			SubShader {
				Pass {
					SetTexture[_MainTex] {
						Combine texture * primary DOUBLE
					}
				}
			}
	}
}