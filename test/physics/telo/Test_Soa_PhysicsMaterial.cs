using Howl.Physics;
using Howl.Physics.Telo;

namespace Howl.Test.Physics.Telo;

public class Test_Soa_PhysicsMaterial
{
    [Fact]
    public void Constructor_Test()
    {
        for(int i = 0; i < 12; i++)
        {
            Soa_PhysicsMaterial soa = new(i);
            Assert_Soa_PhysicsMaterial.LengthEqual(i, soa);
            Assert.False(soa.Disposed);
        }
    }

    [Fact]
    public void Insert_Test()
    {
        for(int i = 0; i < 6; i++)
        {
            Soa_PhysicsMaterial soa = new(i);

            for(int j = 0; j < i; j++)
            {
                float staticFriction = 0.02f + (j*0.01f);
                float kineticFriction =  0.01f + (j*0.01f);
                float density = j+1;
                float restitution = j+2;
                Soa_PhysicsMaterial.Insert(soa, new PhysicsMaterial(staticFriction, kineticFriction, density, restitution), j);
                Assert_Soa_PhysicsMaterial.EntryEqual(staticFriction, kineticFriction, density, restitution, j, soa);
            }
        }
    }

    [Fact]
    public void EnforceNil_Test()
    {
        Soa_PhysicsMaterial soa = new(12);
        Soa_PhysicsMaterial.Insert(soa, new PhysicsMaterial(0.9f,0.8f,0.7f,0.6f), 0);
        Soa_PhysicsMaterial.EnforceNil(soa);
        Assert_Soa_PhysicsMaterial.EntryEqual(0, 0, 0, 0, 0, soa);
    }

    [Fact]
    public void Dispose_Test()
    {
        for(int i = 0; i < 3; i++)
        {
            Soa_PhysicsMaterial soa = new(i);            
            for(int j = 0; j < i; j++)
            {
                float staticFriction = 0.02f + (j*0.01f);
                float kineticFriction =  0.01f + (j*0.01f);
                float density = j+1;
                float restitution = j+2;
                Soa_PhysicsMaterial.Insert(soa, new PhysicsMaterial(staticFriction, kineticFriction, density, restitution), j);
            }
            Soa_PhysicsMaterial.Dispose(soa);
            Assert_Soa_PhysicsMaterial.Disposed(soa);
        }
    } 
}