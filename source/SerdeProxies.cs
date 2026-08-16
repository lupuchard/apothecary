using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Serde;

namespace Apothecary;

public static class ImmutableListProxy {
    public static ISerdeInfo GetSerdeInfo<T>(ISerdeInfo elementInfo) =>
        SerdeInfo.MakeEnumerable(typeof(ImmutableList<T>).ToString(), elementInfo);

    private static class SerCache<T, TProvider> where TProvider : ISerializeProvider<T> {
        public static readonly ISerdeInfo SerdeInfo = GetSerdeInfo<T>(TProvider.Instance.SerdeInfo);
    }

    private static class DeCache<T, TProvider> where TProvider : IDeserializeProvider<T> {
        public static readonly ISerdeInfo SerdeInfo = GetSerdeInfo<T>(TProvider.Instance.SerdeInfo);
    }

    public sealed class Ser<T, TProvider>()
        : SerListBase<Ser<T, TProvider>, T, ImmutableList<T>, TProvider>, ISerializeProvider<ImmutableList<T>>
        where TProvider : ISerializeProvider<T> {
        
        public override ISerdeInfo SerdeInfo => SerCache<T, TProvider>.SerdeInfo;

        protected override ReadOnlySpan<T> GetSpan(ImmutableList<T> value) => value.ToImmutableArray().AsSpan();
    }

    public sealed class De<T, TProvider>()
        : DeListBase<De<T, TProvider>, T, ImmutableList<T>, ImmutableList<T>.Builder, TProvider>
        where TProvider : IDeserializeProvider<T> {
        
        public override ISerdeInfo SerdeInfo => DeCache<T, TProvider>.SerdeInfo;

        protected override ImmutableList<T>.Builder GetBuilder(int? sizeOpt) => ImmutableList.CreateBuilder<T>();

        protected override void Add(ImmutableList<T>.Builder builder, T item) => builder.Add(item);

        protected override ImmutableList<T> ToList(ImmutableList<T>.Builder builder) =>
            builder.ToImmutable();
    }
}
