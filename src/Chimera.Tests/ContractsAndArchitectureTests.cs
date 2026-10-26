namespace Chimera.Tests;

using System.Threading.Tasks;
using Chimera.Core.Common;
using Chimera.Core.Contracts;
using Chimera.Core.MathEngine;
using Chimera.Core.Presentation;
using Xunit;

public class ContractsAndArchitectureTests
{
    private sealed class MockSnapshot : IRenderSnapshot
    {
        public ulong MicroTickIndex { get; init; }
        public ulong SimulationTickIndex { get; init; }
        public float PrimarySliderValue { get; init; }
    }

    [Fact]
    public void ContinuousParameterBank_UsesStronglyTypedSliderId_WithoutAllocation()
    {
        var bank = new ContinuousParameterBank();

        bank.SetSliderValue(SliderId.MaterialDensity, 5000.0f);
        float density = bank.GetSliderValue(SliderId.MaterialDensity);

        Assert.Equal(5000.0f, density);
    }

    [Fact]
    public void DoubleBufferedSnapshot_PublishAndRead_AreAtomicAndThreadSafe()
    {
        var initial = new MockSnapshot { MicroTickIndex = 0, SimulationTickIndex = 0, PrimarySliderValue = 1.0f };
        var doubleBuffer = new DoubleBufferedSnapshot<MockSnapshot>(initial);

        Assert.Equal(0UL, doubleBuffer.Current.MicroTickIndex);
        Assert.Equal(1.0f, doubleBuffer.Current.PrimarySliderValue);

        // Simulate concurrent publishing and reading
        Parallel.For(1, 1000, i =>
        {
            var next = new MockSnapshot
            {
                MicroTickIndex = (ulong)i,
                SimulationTickIndex = (ulong)(i / 60),
                PrimarySliderValue = (float)i
            };
            doubleBuffer.Publish(next);

            var read = doubleBuffer.Current;
            Assert.NotNull(read);
            Assert.True(read.MicroTickIndex >= 0);
        });
    }

    private struct ParticleKinematics
    {
        public float PosX, PosY, PosZ;
        public float VelX, VelY, VelZ;
    }

    [Fact]
    public void ContiguousBuffer_OperatesOverSpan_WithoutHeapAllocation()
    {
        var buffer = new ContiguousBuffer<ParticleKinematics>(16);

        buffer.Add(new ParticleKinematics { PosX = 10f, PosY = 20f, PosZ = 30f });
        buffer.Add(new ParticleKinematics { PosX = 40f, PosY = 50f, PosZ = 60f });

        Assert.Equal(2, buffer.Count);

        var span = buffer.AsSpan();
        Assert.Equal(2, span.Length);

        // Mutate in-place via span
        span[0].PosX += 5.0f;
        Assert.Equal(15.0f, buffer[0].PosX);
    }

    private sealed class DiscreteTickableEntity : ISimulationTickable
    {
        public ulong LastMicroTick { get; private set; }
        public ulong LastSimulationTick { get; private set; }
        public ulong LastMacroTick { get; private set; }

        public void StepMicroTick(ulong microTickIndex) => LastMicroTick = microTickIndex;
        public void StepSimulationTick(ulong simulationTickIndex) => LastSimulationTick = simulationTickIndex;
        public void StepMacroTick(ulong macroTickIndex) => LastMacroTick = macroTickIndex;
    }

    [Fact]
    public void SimulationTickable_OperatesWithDiscreteTickIndices()
    {
        var entity = new DiscreteTickableEntity();
        entity.StepMicroTick(120UL);
        entity.StepSimulationTick(2UL);
        entity.StepMacroTick(1UL);

        Assert.Equal(120UL, entity.LastMicroTick);
        Assert.Equal(2UL, entity.LastSimulationTick);
        Assert.Equal(1UL, entity.LastMacroTick);
    }
}
