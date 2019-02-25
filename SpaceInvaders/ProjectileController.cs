﻿/*
 * Author: Shon Verch
 * File Name: ProjectileController.cs
 * Project Name: SpaceInvaders
 * Creation Date: 02/09/2019
 * Modified Date: 02/23/2019
 * Description: DESCRIPTION
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using SpaceInvaders.ContentPipeline;
using SpaceInvaders.Engine;
using Random = SpaceInvaders.Engine.Random;

namespace SpaceInvaders
{
    public class ProjectileController
    {
        private readonly Dictionary<ProjectileType, List<Projectile>> projectilePrototypes;

        /// <summary>
        /// A list containing all the <see cref="Projectile"/> objects to destroy
        /// at the end of the frame.
        /// <remarks>
        /// This is because if the player fires a projectile while we are traversing the
        /// projectile collection (i.e. during draw or update), a CollectionWasModified
        /// exception would be thrown since the collection was modified during traversal.
        /// In order to get around this, we can modify the list AFTER the traversal.
        /// </remarks>
        /// </summary>
        private readonly List<Projectile> destroyList;

        /// <summary>
        /// The current active projectiles shot by enemies.
        /// </summary>
        private readonly Dictionary<ProjectileType, List<Projectile>> activateProjectiles;

        public ProjectileController()
        {
            activateProjectiles = new Dictionary<ProjectileType, List<Projectile>>();
            projectilePrototypes = new Dictionary<ProjectileType, List<Projectile>>();
            destroyList = new List<Projectile>();

            LoadProjectilePrototypes();
        }

        private void LoadProjectilePrototypes()
        {
            string jsonSource = MainGame.Context.Content.Load<JsonObject>("BulletTypes").JsonSource;
            Projectile[] projectiles = JsonConvert.DeserializeObject<Projectile[]>(jsonSource);
            foreach (Projectile projectile in projectiles)
            {
                if (!projectilePrototypes.ContainsKey(projectile.Type))
                {
                    projectilePrototypes[projectile.Type] = new List<Projectile>();

                    // "Warm-up" our active projectiles dictionary so that we don't have to
                    // create the list later. This means that we don't need to check whether
                    // the collection contains the type when we want to modify it.
                    activateProjectiles[projectile.Type] = new List<Projectile>();
                }

                projectilePrototypes[projectile.Type].Add(projectile);
            }
        }

        private void CreateProjectile(Vector2 postion, Projectile prototype)
        {
            activateProjectiles[prototype.Type].Add(new Projectile(prototype, postion));
        }

        /// <summary>
        /// Fires a player projectile
        /// if one does not exist yet.
        /// </summary>
        public void CreatePlayerProjectile()
        {
            // There already exists a player projectile in the world.
            // A player can only fire a projectile after the previous one
            // has been destroyed.
            if (activateProjectiles.ContainsKey(ProjectileType.Player) && activateProjectiles[ProjectileType.Player].Count > 0) return;

            // The projectile should spawn at the top-centre of the player.
            // The position of the player is the top-left so we just need to
            // horizontally offset it by half of the player width.
            Vector2 offset = new Vector2(MainGame.Context.Player.Texture.Width * MainGame.ResolutionScale * 0.5f, 0);
            Vector2 position = MainGame.Context.Player.Position + offset;

            // Get a random player projectile
            CreateProjectile(position, GetRandomProjectilePrototype(ProjectileType.Player));
        }

        public void CreateEnemyProjectile(Enemy enemy)
        {
            RectangleF enemyRectangle = MainGame.Context.EnemyGroup.GetEnemyWorldRectangle(enemy);
            Vector2 position = enemyRectangle.Position + new Vector2(enemyRectangle.Width / 2, 0);

            CreateProjectile(position, GetRandomProjectilePrototype(ProjectileType.Enemy));
        }

        /// <summary>
        /// Removes the specified projectile from the world.
        /// </summary>
        /// <param name="projectile">The projectile to remove.</param>
        public void Remove(Projectile projectile)
        {
            if (!activateProjectiles.ContainsKey(projectile.Type)) return;
            destroyList.Add(projectile);
        }

        public void Update(float deltaTime)
        {
            if (MainGame.Context.IsFrozen) return;
            ApplyOperationOnProjectiles(projectile => projectile.Update(deltaTime));
            CollectProjectiles();
        }

        private void CollectProjectiles()
        {
            foreach (Projectile projectile in destroyList)
            {
                activateProjectiles[projectile.Type].Remove(projectile);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            ApplyOperationOnProjectiles(projectile => projectile.Draw(spriteBatch));
        }

        private void ApplyOperationOnProjectiles(Action<Projectile> operation)
        {
            foreach (List<Projectile> projectiles in activateProjectiles.Values)
            {
                foreach (Projectile projectile in projectiles)
                {
                    operation(projectile);
                }
            }
        }

        /// <summary>
        /// Retrieves a random projectile prototype with the specified type.
        /// </summary>
        /// <param name="type">The type of the projectile prototype.</param>
        private Projectile GetRandomProjectilePrototype(ProjectileType type) => 
            projectilePrototypes[type][Random.Range(projectilePrototypes[type].Count)];
    }
}
