using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubicSpline
{
    public class Spline3D
    {
        #region Nested Classes
        public static class Curve
        {
            public static Cubic[] CalcCurve(int n, float[] axis)
            {
                float[] gamma = new float[n + 1];
                float[] delta = new float[n + 1];
                float[] d = new float[n + 1];
                Cubic[] c = new Cubic[n];

                #region Gamma Calculation

                gamma[0] = 0.5f;

                for (int i = 1; i < n; ++i)
                {
                    gamma[i] = 1.0f / (4.0f - gamma[i - 1]);
                }

                gamma[n] = 1.0f / (2.0f - gamma[n - 1]);

                #endregion Gamma Calculation

                #region Delta Calculation

                delta[0] = 3.0f * (axis[1] - axis[0]) * gamma[0];

                for (int i = 1; i < n; ++i)
                {
                    delta[i] = (3.0f * (axis[i + 1] - axis[i - 1]) - delta[i - 1]) * gamma[i];
                }

                delta[n] = (3.0f * (axis[n] - axis[n - 1]) - delta[n - 1]) * gamma[n];

                #endregion Delta Calculation

                #region d Calculation

                d[n] = delta[n];

                for (int i = n - 1; i >= 0; --i)
                {
                    d[i] = delta[i] - gamma[i] * d[i + 1];
                }

                #endregion d Calculation

                #region c Calculation

                for (int i = 0; i < n; ++i)
                {
                    float x0 = axis[i];
                    float x1 = axis[i + 1];
                    float d0 = d[i];
                    float d1 = d[i + 1];
                    c[i] = new Cubic(x0, d0, 3.0F * (x1 - x0) - 2.0F * d0 - d1,
                            2.0F * (x0 - x1) + d0 + d1);
                }

                #endregion c Calculation

                return c;
            }
        }

        public class Cubic
        {
            #region Members

            private readonly float _a;
            private readonly float _b;
            private readonly float _c;
            private readonly float _d;

            #endregion Members

            #region Methods

            #region Constructor
            public Cubic(float a, float b, float c, float d)
            {
                _a = a;
                _b = b;
                _c = c;
                _d = d;
            }
            #endregion Constructor

            #region Public Methods

            public float Eval(float u)
            {
                return (((_d * u) + _c) * u + _b) * u + _a;
            }

            #endregion Public Methods

            #endregion Methods
        }

        #endregion Nested Classes

        #region Members

        private readonly int count;
        private readonly Cubic[] xCubics;
        private readonly Cubic[] yCubics;
        private readonly Cubic[] zCubics;

        #endregion Members

        #region Properties

        public int PointCount
        {
            get => count;
        }

        #endregion Properties

        #region Constructor

        public Spline3D(float[][] points)
        {
            count = points.Length;

            float[] xAxis = new float[count];
            float[] yAxis = new float[count];
            float[] zAxis = new float[count];

            Parallel.For(0, count, i =>
            {
                xAxis[i] = points[i][0];
                yAxis[i] = points[i][1];
                zAxis[i] = points[i][2];
            });

            xCubics = Curve.CalcCurve(count - 1, xAxis);
            yCubics = Curve.CalcCurve(count - 1, yAxis);
            zCubics = Curve.CalcCurve(count - 1, zAxis);
        }

        #endregion Constructor

        #region Methods

        public float[] GetPositionAt(float param)
        {
            float[] v = null;
            GetPositionAt(param, out v);
            return v;
        }

        public void GetPositionAt(float param, out float[] result)
        {
            if(param < 0.0f)
            {
                param = 0.0f;
            }

            if(param >= count - 1)
            {
                float val = count - 1;
                int precision = (int)Math.Max(0, -Math.Floor(Math.Log10(val)));
                float ulp = 1.0f / (float)Math.Pow(10.0f, precision);

                param = (count - 1) - ulp;
            }

            // Split
            int ti = (int)param;
            float tf = param - ti;

            result = new float[3];

            // Eval
            result[0] = xCubics[ti].Eval(tf);
            result[1] = yCubics[ti].Eval(tf);
            result[2] = zCubics[ti].Eval(tf);
        }

        #endregion Methods
    }
}
