using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Tut3
{
    class Program
    {
        private class Form : GameWindow
        {
            public Form() : base(512,512, new GraphicsMode(32,24,0,4))
            {
 
            }

            int pgmId;
            int vsId;
            int fsId;

            int attribute_vcol;
            int attribute_vpos;
            int uniform_mview;

            int vbo_position;
            int vbo_color;
            int vbo_mview;

            int ibo_elements;

            float time = 0.0f;

            Vector3[] vertdata;
            Vector3[] coldata;
            Matrix4[] mviewdata;

            int[] indicedata;

            void initProgram()
            {
                pgmId = GL.CreateProgram();
                loadShader("C:/DEV/Teka/Teka/BasicOpenTk/Tut3/vs.glsl", ShaderType.VertexShader, pgmId, out vsId);
                loadShader("C:/DEV/Teka/Teka/BasicOpenTk/Tut3/fs.glsl", ShaderType.FragmentShader, pgmId, out fsId);

                GL.LinkProgram(pgmId);
                Console.WriteLine(GL.GetProgramInfoLog(pgmId));

                attribute_vpos = GL.GetAttribLocation(pgmId, "vPosition");
                attribute_vcol = GL.GetAttribLocation(pgmId, "vColor");
                uniform_mview = GL.GetUniformLocation(pgmId, "modelview");

                if (attribute_vpos == -1 || attribute_vcol == -1 || uniform_mview == -1)
                {
                    Console.WriteLine("Error binding attributes");
                }

                GL.GenBuffers(1, out vbo_position);
                GL.GenBuffers(1, out vbo_color);
                GL.GenBuffers(1, out vbo_mview);

                GL.GenBuffers(1, out ibo_elements);
            }

            void loadShader(String filename, ShaderType type, int program, out int address)
            {
                address = GL.CreateShader(type);
                using (StreamReader sr = new StreamReader(filename))
                {
                    GL.ShaderSource(address, sr.ReadToEnd());
                }
                GL.CompileShader(address);
                GL.AttachShader(program, address);
                Console.WriteLine(GL.GetShaderInfoLog(address));
            }

            protected override void OnLoad(EventArgs e)
            {
                base.OnLoad(e);

                initProgram();

                vertdata = new Vector3[] { new Vector3(-0.8f, -0.8f,  -0.8f),
                    new Vector3(0.8f, -0.8f,  -0.8f),
                    new Vector3(0.8f, 0.8f,  -0.8f),
                    new Vector3(-0.8f, 0.8f,  -0.8f),
                    new Vector3(-0.8f, -0.8f,  0.8f),
                    new Vector3(0.8f, -0.8f,  0.8f),
                    new Vector3(0.8f, 0.8f,  0.8f),
                    new Vector3(-0.8f, 0.8f,  0.8f),
                };

                indicedata = new int[]{
                    //front
                    0, 7, 3,
                    0, 4, 7,
                    //back
                    1, 2, 6,
                    6, 5, 1,
                    //left
                    0, 2, 1,
                    0, 3, 2,
                    //right
                    4, 5, 6,
                    6, 7, 4,
                    //top
                    2, 3, 6,
                    6, 3, 7,
                    //bottom
                    0, 1, 5,
                    0, 5, 4
                };

                coldata = new Vector3[] { new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f), 
                new Vector3( 0f,  1f, 0f),new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f), 
                new Vector3( 0f,  1f, 0f),new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f)}; 


                mviewdata = new Matrix4[]{
                    Matrix4.Identity
                };

                Title = "Hello OpenTk";
                GL.ClearColor(0f, 0f, 1f, 1f);
                GL.PointSize(5f);
            }

            protected override void OnRenderFrame(FrameEventArgs e)
            {
                base.OnRenderFrame(e);

                GL.Viewport(0, 0, Width, Height);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Enable(EnableCap.DepthTest);

                GL.EnableVertexAttribArray(attribute_vpos);
                GL.EnableVertexAttribArray(attribute_vcol);

                GL.DrawElements(BeginMode.Triangles, indicedata.Length, DrawElementsType.UnsignedInt, 0);

                GL.DisableVertexAttribArray(attribute_vpos);
                GL.DisableVertexAttribArray(attribute_vcol);

                GL.Flush();

                SwapBuffers();
            }

            protected override void OnUpdateFrame(FrameEventArgs e)
            {
                base.OnRenderFrame(e);

                time += (float)e.Time;

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_position);
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 0, 0);

                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_color);
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, true, 0, 0);

                GL.UniformMatrix4(uniform_mview, false, ref mviewdata[0]);
                GL.UseProgram(pgmId);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);

                mviewdata[0] = Matrix4.CreateRotationY(0.55f * time) * Matrix4.CreateRotationX(0.15f * time) * Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f) * Matrix4.CreatePerspectiveFieldOfView(1.3f, 360f / 360f, 1.0f, 40.0f);
            }
        }

        static void Main(string[] args)
        {
            using (Form form = new Form())
            {
                form.Run(30, 30);
            }
        }
    }
}
