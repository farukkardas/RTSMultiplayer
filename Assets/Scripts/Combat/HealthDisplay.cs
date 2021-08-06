using System;
using UnityEngine;
using UnityEngine.UI;

namespace Combat
{
    public class HealthDisplay : MonoBehaviour
    {
        [SerializeField] private Health health = null;
        [SerializeField] private GameObject healthBarParent = null;
        [SerializeField] private Image healthBarImage = null;

        private void Awake()
        {
            health.ClientOnHealthUpdated += HandleHealthUpdated;
        }

        private void OnDestroy()
        {
            health.ClientOnHealthUpdated -= HandleHealthUpdated;
        }
        
        private void OnMouseEnter()
        {
            healthBarParent.SetActive(true);
           
        }

        private void OnMouseExit()
        {
            healthBarParent.SetActive(false);
        }

        private void HandleHealthUpdated(int currentHealth, int maxHealth)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}
