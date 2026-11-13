#nullable enable
using afl.EventDirector;

namespace ActorWorkspace.EventCommand
{
    public enum AWEventCommandId
    {
        _Begin = CoreEventCommandId._AWEventCommandId,

        // MEMO: 説明([EventCommandInfo])は Inspector とコマンド定義の JSON に出る

        [EventCommandInfo("アクターのプールに、指定した数のアクターを用意する(読み込む)。用意が終わるまで次へ進まない", Param1 = "アクターの Id(Int)", Param2 = "アクターのカテゴリー(Int)", Param3 = "用意する数(Int)")]
        AddToActorPool,
        [EventCommandInfo("プールからアクターを出す。プールに無いときは読み込んでから出す(SpawnActorAsync)。出し終わるまで次へ進まない。出す位置・親・クラスターは、コードから UserData で渡す", Param1 = "アクターの Id(Int)", Param2 = "アクターのカテゴリー(Int)")]
        SpawnActor,
        [EventCommandInfo("アクターをプールに読み込んでから出す。出し終わるまで次へ進まない。ふつうは SpawnActor から使われる", Param1 = "アクターの Id(Int)", Param2 = "アクターのカテゴリー(Int)", Param3 = "クラスター(Int)。UserData で渡したときはそちらを使う")]
        SpawnActorAsync,
        [EventCommandInfo("クラスターのアクターを消す。すぐ次へ進む", Param1 = "クラスター(Int)。Response を使うときは r: を付ける", Param2 = "消し方(Int)。0 = すぐ消す、1 = アクターに Despawn イベントを送る")]
        DespawnActor,

        _ImmediateBegin,
        _ImmediateEnd,

        _End,
    }
}
#nullable restore