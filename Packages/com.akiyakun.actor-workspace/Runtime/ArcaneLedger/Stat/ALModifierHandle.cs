#nullable enable

namespace ActorWorkspace.ArcaneLedger
{
    // ALStatTable.AddModifier() で足した補正を外すためのもの
    // Dispose() で外す(外したあとにもう一度呼んでも何もしない)
    public readonly struct ALModifierHandle : System.IDisposable
    {
        readonly ALStatTable? table;
        internal int StatId { get; }
        internal int Serial { get; }

        internal ALModifierHandle(ALStatTable table, int statId, int serial)
        {
            this.table = table;
            StatId = statId;
            Serial = serial;
        }

        // 補正がまだ付いているか
        public bool IsValid => table != null && table.Contains(this);

        public void Dispose()
        {
            table?.RemoveModifier(this);
        }
    }
}
#nullable restore
