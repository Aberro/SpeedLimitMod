import { useCallback, useContext, useEffect, useState, useRef } from 'react';
import styled from 'styled-components';

import { LocaleContext } from 'context';
import { getString } from 'localisations';

import Title from './title';

const Container = styled.div`
  padding: 4rem 8rem;
`;

const Gap = styled.div`height: 6rem;`;

const RangeComponent = styled.div`
  padding: 4rem 0;
  width: 100%;
`;

const Track = styled.div`
  background-color: rgba(255, 255, 255, 0.5);
  border-radius: 4rem;
  width: 100%;
  height: 8rem;
  display: flex;
  align-items: center;
  justify-content: flex-start;
  flex-direction: row;
  padding: 0 8rem;
`;

const Filler = styled.div`
  background-color: var(--accentColorNormal);
  box-shadow: var(--accentColorNormal) -8rem 0;
  border-radius: 4rem 0 0 4rem;
  height: 8rem;
`;

const Thumb = styled.div`
  background-color: var(--textColor);
  border-radius: 50%;
  width: 16rem;
  height: 16rem;
  margin-left: -8rem;
`;

export function Range(props: {
    value: number,
    min: number,
    max: number,
    step: number,
    onChange?: (value: number) => void,
    onUpdate?: (value: number) => void
}) {
    const min = props.min;
    const max = props.max;
    const step = props.step

    const [dragging, setDragging] = useState(false);
    const [value, setValue] = useState(0);
    const sliderRef = useRef<HTMLDivElement>(null);

    const getNewValue = (clientX: number) => {
        let sliderLeft = 0;
        let sliderWidth = 0;
        if (sliderRef.current) {
            const rect = sliderRef.current.getBoundingClientRect();
            sliderLeft = rect.left;
            sliderWidth = rect.right - rect.left;
        }
        let newValue = (Math.round((((clientX - sliderLeft) / sliderWidth) * (max - min)) / step) * step) + min;
        if (newValue < min) {
            newValue = min;
        }
        if (newValue > max) {
            newValue = max;
        }
        return newValue;
    };

    const mouseDownHandler = (_event: React.MouseEvent<HTMLElement>) => {
        setDragging(true);
    };
    const mouseUpHandler = useCallback((event: MouseEvent) => {
        const newValue = getNewValue(event.clientX);
        setValue(newValue);
        if (props.onChange) {
            props.onChange(newValue);
        }
        setDragging(false);
    }, [props, getNewValue]);
    const mouseMoveHandler = useCallback((event: MouseEvent) => {
        const newValue = getNewValue(event.clientX);
        setValue(newValue);
        if (props.onUpdate) {
            props.onUpdate(newValue);
        }
    }, [props, getNewValue]);

    useEffect(() => {
        if (dragging) {
            document.body.addEventListener("mouseup", mouseUpHandler);
            document.body.addEventListener("mousemove", mouseMoveHandler);
            return () => {
                document.body.removeEventListener("mouseup", mouseUpHandler);
                document.body.removeEventListener("mousemove", mouseMoveHandler);
            };
        }
    }, [dragging, mouseMoveHandler, mouseUpHandler]);

    useEffect(() => {
        setValue(props.value);
    }, [props.value]);

    const sliderValue = (value - min) / (max - min) * 100;

    return (
        <RangeComponent onMouseDown={mouseDownHandler}>
            <Track ref={sliderRef}>
                <Filler style={{ width: sliderValue + "%" }} />
                <Thumb />
            </Track>
        </RangeComponent>
    );
}
export default function MainPanelRange(props: {
    value: number,
    min: number,
    max: number,
    step: number,
    label: string,
    valuePrefix?: string,
    valueSuffix?: string,
    changeHandler: (value: number) => void
}) {
    const locale = useContext(LocaleContext);
    const [value, setValue] = useState(props.value);
    const changeHandler = props.changeHandler;
    const updateHandler = (value: number) => { setValue(value); };
    useEffect(() => {
        setValue(props.value);
    }, [props.value]);
    return (
        <Container>
            <Title
                title={props.label}
                secondaryText={
                    (props.valuePrefix ? getString(locale, props.valuePrefix) : '')
                    + value
                    + (props.valueSuffix ? getString(locale, props.valueSuffix) : '')} />
            <Gap />
            <Range
                value={props.value}
                min={props.min}
                max={props.max}
                step={props.step}
                onChange={changeHandler}
                onUpdate={updateHandler} />
        </Container>
    );
}