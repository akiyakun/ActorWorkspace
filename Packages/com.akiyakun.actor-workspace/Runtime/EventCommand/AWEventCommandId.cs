#nullable enable
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    [EventCommandIdGroup("AW", order: 100)]
    public enum AWEventCommandId
    {
        _Begin = CoreEventCommandId._AWEventCommandId,

        // MEMO: 説明([EventCommandInfo])は Inspector とコマンド定義の JSON に出る

        [EventCommandInfo("アクターのプールに、指定した数のアクターを用意する(読み込む)。用意が終わるまで次へ進まない", Label = "アクターを用意する", Category = "アクター", Completion = EventCommandCompletion.Wait, Param1 = "アクターの Id(Int)", Param2 = "アクターのカテゴリー(Int)", Param3 = "用意する数(Int)")]
        [EventCommandParam(1, "アクターの Id", Type = EventCommandParamType.Int, Ref = "Actor", RefGroupParam = 2, Description = "カテゴリーごとのマスタの Id。候補はカテゴリー(Param2)で絞る")]
        [EventCommandParam(2, "アクターのカテゴリー", Type = EventCommandParamType.Int, Ref = "ActorCategory")]
        [EventCommandParam(3, "用意する数", Type = EventCommandParamType.Int, Min = 1.0f, Max = 100.0f, Unit = "体")]
        AddToActorPool,
        [EventCommandInfo("プールからアクターを出す。プールに無いときは読み込んでから出す(SpawnActorAsync)。出し終わるまで次へ進まない。出す位置・親・クラスターは、コードから UserData で渡す", Label = "アクターを出す", Category = "アクター", Completion = EventCommandCompletion.Wait, Param1 = "アクターの Id(Int)", Param2 = "アクターのカテゴリー(Int)")]
        [EventCommandParam(1, "アクターの Id", Type = EventCommandParamType.Int, Ref = "Actor", RefGroupParam = 2, Description = "カテゴリーごとのマスタの Id。候補はカテゴリー(Param2)で絞る")]
        [EventCommandParam(2, "アクターのカテゴリー", Type = EventCommandParamType.Int, Ref = "ActorCategory")]
        SpawnActor,
        [EventCommandInfo("アクターをプールに読み込んでから出す。出し終わるまで次へ進まない。ふつうは SpawnActor から使われる", Label = "アクターを読み込んで出す", Category = "アクター", Completion = EventCommandCompletion.Wait, Param1 = "アクターの Id(Int)", Param2 = "アクターのカテゴリー(Int)", Param3 = "クラスター(Int)。UserData で渡したときはそちらを使う")]
        [EventCommandParam(1, "アクターの Id", Type = EventCommandParamType.Int, Ref = "Actor", RefGroupParam = 2, Description = "カテゴリーごとのマスタの Id。候補はカテゴリー(Param2)で絞る")]
        [EventCommandParam(2, "アクターのカテゴリー", Type = EventCommandParamType.Int, Ref = "ActorCategory")]
        [EventCommandParam(3, "クラスター", Type = EventCommandParamType.Int, Description = "UserData で渡したときはそちらを使う")]
        SpawnActorAsync,
        [EventCommandInfo("クラスターのアクターを消す。すぐ次へ進む", Label = "アクターを消す", Category = "アクター", Completion = EventCommandCompletion.Immediate, Param1 = "クラスター(Int)。Response を使うときは r: を付ける", Param2 = "消し方(Int)。0 = すぐ消す、1 = アクターに Despawn イベントを送る")]
        [EventCommandParam(1, "クラスター", Type = EventCommandParamType.Int, Description = "Response を使うときは r: と書く")]
        [EventCommandParam(2, "消し方", Type = EventCommandParamType.Int, Choices = "0=すぐ消す;1=アクターに Despawn イベントを送る")]
        DespawnActor,

        _ImmediateBegin,
        _ImmediateEnd,

        _End,
    }
}
#nullable restore