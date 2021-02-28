namespace PsyPuppets.Gameplay.Characters.Controllers
{
    public interface IAbilityController
    {
        bool IsDead { get; set; }

        System.Action Died { get; set; }

        bool RegisterAbility(Abilities.IAbility ability);
        bool UnRegisterAbility(Abilities.IAbility ability);
    }
}