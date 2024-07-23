import { useState, useEffect, useContext, useRef, CSSProperties } from "react";
import classNames from "classnames";
import styled from 'styled-components';
import { PanelSection, Button } from "cs2/ui";
import { bindValue, useValue, trigger } from "cs2/api";
import { useLocalization } from "cs2/l10n";

import { Bindings } from "types/reinforced";
import { getString } from "localisations";
import { LocaleContext } from 'context';
import Header from 'ui_src/components/header';
import Title from 'ui_src/components/title';
import MainPanelRange from 'ui_src/components/range';
import mod from "mod.json";

interface Props {
    isEditor?: boolean;
}

const unitsBinding = bindValue<string>(mod.id, Bindings.UnitSystem, "km/h");
const averageSpeedBinding = bindValue<number>(mod.id, Bindings.AverageSpeed, -1);
const roadNameBinding = bindValue<string>(mod.id, Bindings.Name, '');
const ToolPanel = styled.div`
    width: 330rem;
    position: absolute;
    top: calc(10rem + var(--floatingToggleSize));
    left: 0rem;
`;
const ToolPanelBody = styled.div`
    background-color: var(--panelColorNormal);
    backdrop-filter: var(--panelBlur);
    color: var(--textColor);
    border-radius: 0rem 0rem 4rem 4rem;
    flex: 1;
    position: relative;
    padding: 6rem;
`;
const FlexboxComponent = styled.div`
    display: flex;
flex-flow: row nowrap;`;
const ButtonComponent = styled.div`
    padding: 3rem;
    margin: 3rem;
    border-radius: 3rem;
    color: var(--accentColorLighter);
    background-color: var(--toolbarFieldColor);
    flex: 1;
    text-align: center;`;

export const SpeedLimitTool = ({ isEditor }: Props) => {
    const units = useValue(unitsBinding);
    const averageSpeed = useValue(averageSpeedBinding);
    const roadName = useValue(roadNameBinding);

    const locale = useContext(LocaleContext);

    const [speedValue, setSpeedValue] = useState(averageSpeed);
    const [top, setTop] = useState(-999);
    const [left, setLeft] = useState(-999);
    const [dragging, setDragging] = useState(false);
    const containerRef = useRef<any>(null);
    const [speeds, setSpeeds] = useState([30, 40, 50, 60, 80, 100, 120]);

    useEffect(() => setSpeedValue(averageSpeed), [averageSpeed]);
    useEffect(() => setSpeeds(units == "kph" ? [30, 40, 50, 60, 80, 100, 120, 140] : [25, 30, 35, 45, 55, 65, 70, 90]), [units]);

    const onClose = () => {
        const data = { type: "toggle_visibility", id: "speed-limit-editor" };
        const event = new CustomEvent('hookui', { detail: data });
        window.dispatchEvent(event);
    }
    const handleSpeedChange = (s: number) => {
        trigger(mod.id, Bindings.SetSpeedLimit, s);
    }
    const handleReset = () => {
        trigger(mod.id, Bindings.Reset);
    }

    const mouseDownHandler = (_event: React.MouseEvent<HTMLElement>) => {
        if (containerRef.current) {
            const rect = containerRef.current.getBoundingClientRect();
            setTop(rect.top);
            setLeft(rect.left);
            setDragging(true);
        }
    };
    const mouseUpHandler = (_event: MouseEvent) => {
        setDragging(false);
    };
    const mouseMoveHandler = (event: MouseEvent) => {
        setTop((prev) => prev + event.movementY);
        setLeft((prev) => prev + event.movementX);
    };

    useEffect(() => {
        if (dragging) {
            document.body.addEventListener("mouseup", mouseUpHandler);
            document.body.addEventListener("mousemove", mouseMoveHandler);
            return () => {
                document.body.removeEventListener("mouseup", mouseUpHandler);
                document.body.removeEventListener("mousemove", mouseMoveHandler);
            };
        }
    }, [dragging]);

    let minMax = { min: 1, max: 200 };
    if (units === 'mph')
        minMax = { min: 1, max: 124 }

    const toolPanelStyle: React.CSSProperties = {
        display: "block"
    };
    if (top >= -200 && left >= -200) {
        toolPanelStyle.top = top;
        toolPanelStyle.left = left;
    }

    const SetSpeedButton = (value: number) =>
    (<ButtonComponent
        onClick={(e) => {
            setSpeedValue(value);
            handleSpeedChange(value);
        }}>{value.toString()}</ButtonComponent>)

    return (
        <ToolPanel
            ref={containerRef}
            style={toolPanelStyle}>
            <Header
                title={getString(locale, "SpeedLimitEditor")}
                image="Media/Game/Policies/HighSpeedHighways.svg"
                onMouseDown={mouseDownHandler} />
            <ToolPanelBody>

                {/*<Title title={averageSpeed.toString()} />*/}
                {roadName === '' && averageSpeed === -1
                    ? <Title title={'No road selected'} />
                    : <div>
                        <Title title={roadName} />
                        <FlexboxComponent>
                            {speeds.map(SetSpeedButton)}
                        </FlexboxComponent>
                        <ButtonComponent onClick={handleReset}>{getString(locale, "Reset")}</ButtonComponent>
                        <MainPanelRange
                            value={speedValue}
                            min={minMax.min}
                            max={minMax.max}
                            step={1}
                            label={getString(locale, "SpeedLimit")}
                            valueSuffix={units}
                            changeHandler={handleSpeedChange}
                        />
                    </div>}
            </ToolPanelBody>
        </ToolPanel>);
}