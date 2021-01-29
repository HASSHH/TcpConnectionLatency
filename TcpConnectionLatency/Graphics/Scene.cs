using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TcpConnectionLatency.Graphics
{
    class Scene
    {
        private bool initDone;
        private int vpMatrixUniformLocation;
        private Matrix4 viewMatrix = Matrix4.Identity;
        private Matrix4 projectionMatrix = Matrix4.Identity;

        public Scene(GLControl glc)
        {
            initDone = false;
            GLControl = glc;

            Values = new List<uint>();
            Height = 100;
        }

        public GLControl GLControl { get; private set; }
        public List<uint> Values { get; set; }
        public float Height { get; set; }

        public void Paint()
        {
            if (initDone)
            {
                Draw();
                GLControl.SwapBuffers();
            }
            else
                Init();
        }

        public void Resize()
        {
            GL.Viewport(0, 0, GLControl.Width, GLControl.Height);
        }

        private void Draw()
        {
            ComputePojection();
            Matrix4 vpMatrix = viewMatrix * projectionMatrix;
            GL.UniformMatrix4(vpMatrixUniformLocation, false, ref vpMatrix);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if(Values != null && Values.Count > 1)
            {
                GL.Begin(PrimitiveType.LineStrip);
                for(int i = Values.Count-1; i >= 0; --i)
                {
                    GL.Vertex3(i - Values.Count + 1, Values[i], 0);
                }
                GL.End();
            }
        }

        private void Init()
        {
            GL.ClearColor(0.15f, 0.15f, 0.15f, 1f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.LineWidth(3);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, ReadShaderString("basic_shader.frag"));
            GL.CompileShader(fragmentShader);
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, ReadShaderString("basic_shader.vert"));
            GL.CompileShader(vertexShader);

            string vinfo = GL.GetShaderInfoLog(vertexShader);
            string finfo = GL.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrWhiteSpace(vinfo) || !string.IsNullOrWhiteSpace(finfo))
                throw new Exception("Shader compile exception");

            int shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            GL.UseProgram(shaderProgram);
            vpMatrixUniformLocation = GL.GetUniformLocation(shaderProgram, "vp_matrix");

            initDone = true;
        }

        private void ComputePojection()
        {
            projectionMatrix = Matrix4.CreateOrthographicOffCenter(-60, 1, -1, Height+1, -0.1f, 1f);
        }

        private string ReadShaderString(string fileName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TcpConnectionLatency.Graphics.Shaders." + fileName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
