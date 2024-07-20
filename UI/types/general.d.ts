
type MainPanelItem = MainPanelItemTitle | MainPanelItemRange;

interface MainPanelItemTitle {
	itemType: "title",
	title: string,
	secondaryText?: string
}

interface MainPanelItemRange {
	itemType: "range",
	key: string,
	label: string,
	value: number,
	valuePrefix: string,
	valueSuffix: string,
	min: number,
	max: number,
	step: number,
	changeHandler: (value: number) => void
}