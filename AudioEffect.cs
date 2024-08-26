namespace XB { // namespace open
// AudioEffect is a very simple class used for effects that are instantiated
// and intended to be destroyed, so no re-use of effects that use this
public partial class AudioEffect : Godot.AudioStreamPlayer {
    public override void _Ready() {
        Play();
    }

    //NOTE[ALEX]: set looping mode of audio for each file separately when importing
    public override void _PhysicsProcess(double delta) {
        if (!Playing) { QueueFree(); }
    }
}
} // namespace close
