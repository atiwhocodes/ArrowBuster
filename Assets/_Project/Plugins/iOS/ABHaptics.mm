// Arrow Buster haptics bridge (D-032). Called from HapticsService via DllImport("__Internal").
// kind: 0 Light, 1 Medium, 2 Heavy, 3 Success, 4 Failure, 5 Selection (matches ArrowBuster.HapticKind).
#import <UIKit/UIKit.h>

extern "C" void ABHaptics_Play(int kind)
{
    if (@available(iOS 10.0, *))
    {
        switch (kind)
        {
            case 0: { UIImpactFeedbackGenerator *g = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight]; [g impactOccurred]; break; }
            case 1: { UIImpactFeedbackGenerator *g = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium]; [g impactOccurred]; break; }
            case 2: { UIImpactFeedbackGenerator *g = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy]; [g impactOccurred]; break; }
            case 3: { UINotificationFeedbackGenerator *g = [[UINotificationFeedbackGenerator alloc] init]; [g notificationOccurred:UINotificationFeedbackTypeSuccess]; break; }
            case 4: { UINotificationFeedbackGenerator *g = [[UINotificationFeedbackGenerator alloc] init]; [g notificationOccurred:UINotificationFeedbackTypeError]; break; }
            default: { UISelectionFeedbackGenerator *g = [[UISelectionFeedbackGenerator alloc] init]; [g selectionChanged]; break; }
        }
    }
}
