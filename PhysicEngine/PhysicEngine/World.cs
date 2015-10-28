using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PhysicEngine
{
    class World
    {
        List<Body> bodies = new List<Body>();
        public Vector2 gravity = new Vector2(0, 10);
        int integrate_cnt = 10;
        float mult = 500.0f; //1000 to real simulation
        List<Vector2> box(int width, int height)
        {
            List<Vector2> box = new List<Vector2>();
            box.Add(new Vector2(-width, -height));
            box.Add(new Vector2(width, -height));
            box.Add(new Vector2(width, height));
            box.Add(new Vector2(-width, height));
            return box;
        }
        List<Vector2> boxS(int w)
        {
            List<Vector2> box = new List<Vector2>();
            float k = 1.41f;
            box.Add(new Vector2(-w, -w));
            box.Add(new Vector2(0, -w*k));
            box.Add(new Vector2(w, -w));
            box.Add(new Vector2(w*k, 0));
            box.Add(new Vector2(w, w));
            box.Add(new Vector2(0, w*k));
            box.Add(new Vector2(-w, w));
            box.Add(new Vector2(-w * k, 0));
            return box;
        }
        public void InitWorld()
        {
            //World template
            //bodies.Add(new Body(new Vector2(90, 300), new Vector2(0, 0), 1, box(10, 10), false));
            for (int j = 0; j < 5; j++)
            for (int i = 0; i < 5; i++)
            {
                bodies.Add(new Body(new Vector2(170 + 40 * i, 300-42*j), new Vector2((/*13 * i % 7*/0) / 2f, (/*13 * i % 5*/0) / 1f), 1, boxS(10), false));
                //bodies[bodies.Count - 1].angle = 0.95f * (i+j);
            }
            //bodies.Add(new Body(new Vector2(450, 240), new Vector2(-50, -1), 100, boxS(20), false));
            bodies.Add(new Body(new Vector2(70, 400), new Vector2(0, 0), 1, box(500, 40), true));
            bodies[bodies.Count - 1].angle = -1.50f;
            bodies.Add(new Body(new Vector2(600, 400), new Vector2(0, 0), 1, box(500, 40), true));
            bodies[bodies.Count - 1].angle = -1.50f;
            bodies.Add(new Body(new Vector2(195, 400), new Vector2(0, 0), 1, box(500, 40), true));
            bodies[bodies.Count - 1].angle = 0.0f;
            bodies.Add(new Body(new Vector2(195,50), new Vector2(0, 0), 1, box(500, 40), true));
            bodies[bodies.Count - 1].angle = 0.0f;
            //bodies.Add(new Body(new Vector2(300, 400), new Vector2(0, 0), 1, box(300, 40), true));
            /*bodies.Add(new Body(new Vector2(300, 500), new Vector2(0, 0), 1, box(140, 140), true));
            bodies[bodies.Count - 1].angle = 0.7f;*/
            //bodies.Add(new Body(new Vector2(340, 200), new Vector2(0, 0), 1, box(200, 10), false));
           
        }
        bool BroadPhase(Body a, Body b)
        {
            Vector2 maxa, mina, maxb, minb;
            maxa = mina = a.shape[0] + a.position;
            maxb = minb = b.shape[0] + b.position;

            #region Cycles
            foreach (Vector2 v in a.shape)
            {
                mina.X = Math.Min(mina.X, v.X + a.position.X);
                mina.Y = Math.Min(mina.Y, v.Y + a.position.Y);
                maxa.X = Math.Max(maxa.X, v.X + a.position.X);
                maxa.Y = Math.Max(maxa.Y, v.Y + a.position.Y);
            }
            foreach (Vector2 v in b.shape)
            {
                minb.X = Math.Min(minb.X, v.X + b.position.X);
                minb.Y = Math.Min(minb.Y, v.Y + b.position.Y);
                maxb.X = Math.Max(maxb.X, v.X + b.position.X);
                maxb.Y = Math.Max(maxb.Y, v.Y + b.position.Y);
            }
            #endregion

            if (maxa.X >= minb.X && mina.X <= maxb.X && maxa.Y >= minb.Y && mina.Y <= maxb.Y)
                return true;
            if (maxb.X >= mina.X && minb.X <= maxa.X && maxb.Y >= mina.Y && minb.Y <= maxa.Y)
                return true;
            return false;
        }
        public World()
        {

        }
        Vector2 rotate(Vector2 a, float angle)
        {
            return new Vector2((float)(a.X * Math.Cos(angle) + a.Y * Math.Sin(angle)), (float)(-a.X * Math.Sin(angle) + a.Y * Math.Cos(angle)));
        }
        void Solve(Body a, Body b, float time)
        {
            //Bad code
            Vector2 collision = Intersect(a, b);
            if (collision.Length() > 2000) return;
            
            //A NORMAL CALCULATION
            Vector2 edgea = Vector2.Zero; int cnta = 0; float dista = 10000;
            int c = a.shape.Count;
            float angle = (float)Math.Acos(-1) / 2;
            // DIST MINIMAZING
            for (int i = 0; i < c; i++)
            {
                float d = area(a.shape[i], a.shape[(i + 1) % c], collision - a.position) / (a.shape[i] - a.shape[(i + 1) % c]).Length();
                if (d < dista)
                    dista = d;
            }
            for (int i = 0; i < c; i++)
            {
                float d = area(a.shape[i], a.shape[(i + 1) % c], collision - a.position) / (a.shape[i] - a.shape[(i + 1) % c]).Length();
                if (Math.Abs(d - dista) < 1e-3 && cnta < 1)
                {
                    Vector2 n = rotate(a.shape[i] - a.shape[(i + 1) % c], angle); n.Normalize();
                    edgea += n;
                    cnta++;
                }
            }
            //B NORMAL CALCULATION
            Vector2 edgeb = Vector2.Zero; int cntb = 0; float distb = 10000;
            c = b.shape.Count;
            // DIST MINIMAZING
            for (int i = 0; i < c; i++)
            {
                float d = area(b.shape[i], b.shape[(i + 1) % c], collision - b.position) / (b.shape[i] - b.shape[(i + 1) % c]).Length();
                if (d < distb)
                    distb = d;
            }
            for (int i = 0; i < c; i++)
            {
                float d = area(b.shape[i], b.shape[(i + 1) % c], collision - b.position) / (b.shape[i] - b.shape[(i + 1) % c]).Length();
                if (Math.Abs(d - distb) < 1e-3 && cntb < 1)
                {
                    Vector2 n = rotate(b.shape[i] - b.shape[(i + 1) % c], angle); n.Normalize();
                    edgeb += n;
                    cntb++;
                }
            }
            float cor = a.cor * b.cor / (a.cor + b.cor);
            float forcea = cor * (distb + dista) / 100;
            float forceb = cor * (dista + distb) / 100;
            //Some magic & phusic
            if (Math.Abs(dista) < 1e-4) edgea = -edgeb;
            if (Math.Abs(distb) < 1e-4) edgeb = -edgea;
            Vector2 va = a.velocity; va.Normalize();
            Vector2 vb = b.velocity; vb.Normalize();
            if (p1(va, edgea) >= 0) 
                forcea *= a.bounce;
            if (p1(vb, edgeb) >= 0) 
                forceb *= b.bounce;

            a.ApplyImpulse(collision - a.position, edgea * forcea / cnta);
            b.ApplyImpulse(collision - b.position, edgeb * forceb / cntb);

            return;
        }

        float p(Vector2 a, Vector2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
        float p1(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }
        float area(Vector2 a, Vector2 b, Vector2 c)
        {
            return Math.Abs(p(b - a, c - a)) / 2;
        }
        bool isin(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float ar = area(a, b, c) - area(a, b, d) - area(a, c, d) - area(c, b, d);
            if (Math.Abs(ar) < 1e-3) return true;
            return false;
        }
        Vector2 Intersect(Body a, Body b)
        {
            Vector2 summary = Vector2.Zero;
            int cnt = 0;
            foreach (Vector2 v in a.shape)
            {
                int c = b.shape.Count;
                bool In = false;
                for (int i = 0; i < c - 2; i++)
                {
                    if (isin(b.shape[0], b.shape[(i + 1) % c], b.shape[(i + 2) % c], v + a.position - b.position))
                        In = true;
                }
                if (In)
                {
                    summary += v + a.position;
                    cnt++;
                }
            }
            foreach (Vector2 v in b.shape)
            {
                int c = a.shape.Count;
                bool In = false;
                for (int i = 0; i < c - 2; i++)
                {
                    if (isin(a.shape[0], a.shape[(i + 1) % c], a.shape[(i + 2) % c], v + b.position - a.position))
                        In = true;

                }
                if (In)
                {
                    summary += v + b.position;
                    cnt++;
                }
            }

            if (cnt == 0) return new Vector2(100000, 100000);
            summary /= cnt;
            return summary;
        }

        public void Update(GameTime time)
        {
            float elapsedTime = time.ElapsedGameTime.Milliseconds / mult / integrate_cnt;
            for (int iteration = 0; iteration < integrate_cnt; iteration++)
            {
                for (int i = 0; i < bodies.Count; i++)
                {
                    if (bodies[i].position.Length() > 2000) continue;
                    bodies[i].ApplyImpulse(Vector2.Zero, gravity * elapsedTime * bodies[i].mass);
                    bodies[i].Update(elapsedTime);
                    for (int j = i + 1; j < bodies.Count; j++)
                    {
                        if ((bodies[i].invmass == 0 || bodies[j].invmass == 0) || (bodies[i].position - bodies[j].position).Length() < 30)
                        if (BroadPhase(bodies[i], bodies[j]))
                        {
                            Solve(bodies[i], bodies[j], elapsedTime);
                        }
                    }
                }
            }
        }


        void line(SpriteBatch sb, Vector2 a, Vector2 b, Texture2D t)
        {
            #region Line
            int x0 = (int)(a.X);
            int y0 = (int)(a.Y);
            int x1 = (int)(b.X);
            int y1 = (int)(b.Y);
            bool steep = false;
            if (Math.Abs(x0 - x1) < Math.Abs(y0 - y1))
            {
                int d = y0;
                y0 = x0;
                x0 = d;

                d = y1;
                y1 = x1;
                x1 = d;
                steep = true;
            }
            if (x0 > x1)
            {
                int d = x0;
                x0 = x1;
                x1 = d;
                d = y0;
                y0 = y1;
                y1 = d;
            }
            int dx = x1 - x0;
            int dy = y1 - y0;
            int derror2 = Math.Abs(dy) * 2;
            int error2 = 0;
            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                if (steep)
                {
                    sb.Draw(t, new Rectangle(y, x, 1, 1), Color.White);
                }
                else
                {
                    sb.Draw(t, new Rectangle(x, y, 1, 1), Color.White);
                }
                error2 += derror2;
                if (error2 > dx)
                {
                    y += (y1 > y0 ? 1 : -1);
                    error2 -= dx * 2;
                }
            }
            #endregion
        }
        public void Draw(SpriteBatch sb, Texture2D t)
        {
            foreach (Body b in bodies)
            {
                if (b.position.Length() > 2000) continue;
                for (int i = 0; i < b.shape.Count; i++)
                {
                    line(sb, b.shape[i] + b.position, b.shape[(i + 1) % b.shape.Count] + b.position, t);
                }
            }
        }



    }
}
