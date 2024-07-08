namespace XB { // namespace open
public partial class AudioEffect : Godot.AudioStreamPlayer {
    public override void _Ready() {
        Play();
    }

    //NOTE[ALEX]: set looping mode of audio for each file separately when importing
    public override void _PhysicsProcess(double delta) {
        if (!Playing) QueueFree();
    }
}
} // namespace close
