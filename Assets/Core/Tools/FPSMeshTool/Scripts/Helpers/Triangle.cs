using UnityEngine;
using System.Collections;

namespace CBG.FPSMeshTool {
    public class Triangle {
        public int[] verts = new int[3];
        public int v1 {
            get { return verts[0]; }
            set { verts[0] = value; }
        }
        public int v2 {
            get { return verts[1]; }
            set { verts[1] = value; }
        }
        public int v3 {
            get { return verts[2]; }
            set { verts[2] = value; }
        }
        public Triangle() {
            v1 = v2 = v3 = 0;
        }

        public Triangle(int a, int b, int c) {
            v1 = a;
            v2 = b;
            v3 = c;
        }
        public Triangle(int[] v, int index = 0) {
            if (v.Length >= index + 3) {
                v1 = v[index];
                v2 = v[index + 1];
                v3 = v[index + 2];
            }
        }
        public int this[int i] {
            get {
                return verts[i];
            }
            set {
                verts[i] = value;
            }
        }

    }

}