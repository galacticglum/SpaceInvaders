﻿/*
 * Author: Shon Verch
 * File Name: Enemy.cs
 * Project Name: SpaceInvaders
 * Creation Date: 02/05/2019
 * Modified Date: 02/23/2019
 * Description: A data-class for storing enemy data.
 */

using Microsoft.Xna.Framework;

namespace SpaceInvaders
{
    /// <summary>
    /// A data-class for storing enemy data.
    /// </summary>
    public class Enemy
    {
        /// <summary>
        /// The position of this <see cref="Enemy"/> in the <see cref="EnemyGroup"/> grid.
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// The type of this <see cref="Enemy"/>.
        /// </summary>
        public EnemyType Type { get; }

        /// <summary>
        /// Indicates whether this <see cref="Enemy"/> is still in the game.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The time, in seconds, until this <see cref="Enemy"/> should attack.
        /// </summary>
        public float AttackTime { get; set; }

        /// <summary>
        /// Indicates whether this <see cref="Enemy"/> has attacked.
        /// </summary>
        public bool HasAttacked { get; set; } = false;

        /// <summary>
        /// Initializes a new <see cref="Enemy"/>.
        /// </summary>
        /// <param name="position">The position of this <see cref="Enemy"/> in the <see cref="EnemyGroup"/> grid.</param>
        /// <param name="type">The type of this <see cref="Enemy"/>.</param>
        public Enemy(Vector2 position, EnemyType type)
        {
            Position = position;
            Type = type;
            Active = true;
            AttackTime = 0;
        }
    }
}
