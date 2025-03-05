using UnityEngine;

namespace Lustie.UnityDocfx.Tests
{
    /// <summary>
    /// Represents an enemy in the game
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        /// <summary>
        /// The health of the enemy
        /// </summary>
        public int health = 100;

        /// <summary>
        /// The armor of the enemy
        /// </summary>
        public float armor { get; set; }

        /// <summary>
        /// Move the enemy
        /// </summary>
        /// <param name="direction">move direction</param>
        public void Move(Vector2 direction)
        {

        }
    }
}
