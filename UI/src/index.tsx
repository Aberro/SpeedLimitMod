import { ModRegistrar } from "cs2/modding";
import { SpeedLimitButton } from "ui_src/SpeedLimitButton";

const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.append('GameTopLeft', SpeedLimitButton);
}

export default register;