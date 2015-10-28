using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace PhysicEngine
{
    class Body
    {

        public Vector2 position;
        public float angle;
        public Vector2 velocity;
        public float angular_velocity;

        public float mass, invmass;
        public float angular_mass, invangular_mass;

        public float friction;
        public float bounce;

        public float cor; //coefficient of restitution H/m
        public List<Vector2> static_shape = new List<Vector2>();
        public List<Vector2> shape = new List<Vector2>();

        public Body()
        {
            position = Vector2.Zero;
            angle = 0;
            velocity = Vector2.Zero;
            invmass = mass = 1;
            invangular_mass = angular_mass = 1;
            angular_velocity = 0;
            friction = 0.5f;
            bounce = 0.5f;
            cor = 200;
            shape.Add(new Vector2(0, 0));
        }
        public Body(Vector2 pos, Vector2 vel, float m, List<Vector2> s, bool is_static)
        {
            position = pos;
            velocity = vel;
            angle = 0.0f;
            if (is_static)
            {
                invmass = 0;
                mass = 1000000000;
                invangular_mass = 0;
                angular_mass = 1000000000;
            }
            else
            {
                mass = m;
                invmass = 1 / mass;
                angular_mass = 25;
                invangular_mass = 1 / angular_mass;
            }
            angular_velocity = 0;
            friction = 0.5f;
            bounce = 0.1f;
            cor = 1000;
            foreach (Vector2 v in s)
            {
                shape.Add(v);
                static_shape.Add(v);
            }
        }

        public void ApplyImpulse(Vector2 relative_position, Vector2 impulse)
        {
            if (invmass == 0) return;
            if (relative_position.Length() == 0f)
                velocity += impulse * invmass;
            else
            {
                float product1 = -relative_position.X * impulse.X - relative_position.Y * impulse.Y;
                float product2 = -relative_position.X * impulse.Y + relative_position.Y * impulse.X;
                Vector2 r = impulse; r.Normalize();
                velocity += r * product1 * invmass/relative_position.Length();
                angular_velocity += product2 * invangular_mass/100;
            }
        }
        public void Update(float time)
        {
            position += velocity*time;
            angle += angular_velocity*time;
            for (int i = 0; i < static_shape.Count; i++)
            {
                shape[i] = new Vector2(
                    (float)
                    (static_shape[i].X * Math.Cos(angle) +
                     static_shape[i].Y * Math.Sin(angle)),
                    (float)
                    (-static_shape[i].X * Math.Sin(angle) +
                     static_shape[i].Y * Math.Cos(angle)));
            }
        }
    }
}
