using CodeBase.StaticData.Enemy;
using CodeBase.StaticData.Level;
using CodeBase.StaticData.ProjectileTrace;
using CodeBase.StaticData.Weapon;

namespace CodeBase.Services.StaticData
{
    public interface IStaticDataService : IService
    {
        void Load();
        EnemyStaticData ForEnemy(EnemyTypeId typeId);
        HeroWeaponStaticData ForHeroWeapon(HeroWeaponTypeId typeId);
        EnemyWeaponStaticData ForEnemyWeapon(EnemyWeaponTypeId typeId);
        ProjectileTraceStaticData ForProjectileTrace(ProjectileTraceTypeId projectileTraceTypeId);

        LevelStaticData ForLevel(string sceneKey);
        // WindowStaticData ForWindow(WindowId windowId);
    }
}