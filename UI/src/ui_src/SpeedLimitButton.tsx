import { bindValue, useValue, trigger } from "cs2/api";
import classNames from "classnames";
import { Button } from "cs2/ui";
import { SpeedLimitTool } from "ui_src/speedLimitTool";
import mod from "../../mod.json";
import { Bindings } from "types/reinforced";
import styles from "ui_src/SpeedLimitButton.module.scss";

const toolActiveBinding = bindValue<boolean>(mod.id, Bindings.SpeedLimitToolActive, false);

export const SpeedLimitButton = () => {
	const speedLimitToolActive = useValue(toolActiveBinding);

	return (
		<>
			<Button
				src='Media/Game/Policies/HighSpeedHighways.svg'
				variant="floating"
				className={classNames({ [styles.selected]: speedLimitToolActive }, styles.toggle)}
				onSelect={() => trigger(mod.id, Bindings.SelectTool) }/>
			{speedLimitToolActive && (<SpeedLimitTool />)}
		</>
	);
}
