using UnityEngine;

public class WeaponItem : Item
{
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private string weaponName;

    protected override void OnPickup()
    {
        base.OnPickup();  // Call base class OnPickup first

        // Check if this is the axe and track its pickup
        if (weaponName == "Axe" && FirstTimeInteractionTracker.Instance != null)
        {
            FirstTimeInteractionTracker.Instance.OnAxePickup();
        }
    }

    public override void UseItem()
    {
        if (weaponController != null)
        {
            if (!weaponController.IsWeaponRaised())
            {
                weaponController.ToggleWeaponPosition();
            }
        }
    }

    public WeaponController GetWeaponController()
    {
        return weaponController;
    }

    public override string GetItemName()
    {
        return weaponName;
    }

    public void ForceToLowered()
    {
        if (weaponController != null && weaponController.IsWeaponRaised())
        {
            weaponController.ToggleWeaponPosition();
        }
    }
} 